using Homecare.Application.Constants;
using Homecare.Application.DTOs.Referral;
using Homecare.Application.Interfaces.Referral;
using Homecare.Data;
using Homecare.Domain.Entities;
using Homecare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Homecare.Application.Services.Referral;

public class ReferralService : IReferralService
{
    private readonly AppDbContext _context;

    public ReferralService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool success, string message)> ApplyReferralAtSignupAsync(
        string referralCode, int newCustomerId)
    {
        var code = referralCode.Trim().ToUpper();

        var referrer = await _context.Customers
            .FirstOrDefaultAsync(c => c.ReferralCode == code);

        if (referrer == null)
            return (false, "Invalid referral code.");

        if (referrer.Id == newCustomerId)
            return (false, "You cannot use your own referral code.");

        if (referrer.ReferralUseCount >= ReferralConstants.MaxReferrals)
            return (false, "This referral code has reached its maximum usage limit.");

        bool alreadyUsed = await _context.ReferralUses
            .AnyAsync(r => r.RefereeId == newCustomerId);

        if (alreadyUsed)
            return (false, "You have already used a referral code.");

        _context.ReferralUses.Add(new ReferralUse
        {
            ReferrerId = referrer.Id,
            RefereeId = newCustomerId,
            ReferralCode = code,
            Status = ReferralStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });

        referrer.ReferralUseCount++;
        await _context.SaveChangesAsync();

        return (true,
            $"Referral code applied! You'll get {ReferralConstants.RefereeFirstOrderDiscountPct}% off your first booking.");
    }

    public async Task ProcessReferrerRewardAsync(int completedBookingId)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == completedBookingId);

        if (booking == null) return;

        var referralUse = await _context.ReferralUses
            .Include(r => r.Referrer)
            .FirstOrDefaultAsync(r =>
                r.RefereeId == booking.CustomerId &&
                r.Status == ReferralStatus.Pending);

        if (referralUse == null) return;

        int completedCount = await _context.Bookings
            .CountAsync(b =>
                b.CustomerId == booking.CustomerId &&
                b.BookingStatus == BookingStatus.Completed);

        if (completedCount != 1) return;

        referralUse.Status = ReferralStatus.Rewarded;
        referralUse.RewardBookingId = completedBookingId;
        referralUse.RewardedAt = DateTime.UtcNow;

        await EnsureWalletAsync(referralUse.ReferrerId);

        var referrerWallet = await _context.CustomerWallets
            .FirstAsync(w => w.CustomerId == referralUse.ReferrerId);

        referrerWallet.Balance += ReferralConstants.ReferrerCredit;
        referrerWallet.ModifiedAt = DateTime.UtcNow;

        _context.WalletTransactions.Add(new WalletTransaction
        {
            WalletId = referrerWallet.Id,
            Amount = ReferralConstants.ReferrerCredit,
            Type = WalletTransactionType.Credit,
            Description = "Referral reward — your referred user completed their first booking",
            ReferenceId = completedBookingId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetRefereeFirstOrderDiscountAsync(
        int customerId, decimal servicePrice, decimal couponDiscount)
    {
        if (couponDiscount > 0) return 0;

        bool hasPendingReferral = await _context.ReferralUses
            .AnyAsync(r => r.RefereeId == customerId && r.Status == ReferralStatus.Pending);

        if (!hasPendingReferral) return 0;

        bool hasAnyPriorBooking = await _context.Bookings
            .AnyAsync(b =>
                b.CustomerId == customerId &&
                b.BookingStatus != BookingStatus.Cancelled &&
                b.BookingStatus != BookingStatus.Failed);

        if (hasAnyPriorBooking) return 0;

        decimal calculatedDiscount = Math.Round(servicePrice * ReferralConstants.RefereeFirstOrderDiscountPct / 100, 2);

        decimal finalDiscount = Math.Min(calculatedDiscount, ReferralConstants.RefereeFirstOrderDiscountMaxAmount);

        return finalDiscount;
    }

    public async Task<decimal> UseWalletAsync(
        int customerId, decimal servicePrice, decimal currentTotal, int bookingId)
    {
        var wallet = await _context.CustomerWallets
            .FirstOrDefaultAsync(w => w.CustomerId == customerId);

        if (wallet == null || wallet.Balance <= 0) return 0;

        decimal cap = Math.Round(servicePrice * ReferralConstants.WalletUsageCapPct / 100, 2);

        decimal amountToUse = Math.Min(wallet.Balance, cap);

        if (amountToUse <= 0) return 0;

        wallet.Balance -= amountToUse;
        wallet.ModifiedAt = DateTime.UtcNow;

        _context.WalletTransactions.Add(new WalletTransaction
        {
            WalletId = wallet.Id,
            Amount = amountToUse,
            Type = WalletTransactionType.Debit,
            Description = $"Wallet credit applied to booking #{bookingId}",
            ReferenceId = bookingId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return amountToUse;
    }

    public async Task EnsureWalletAsync(int customerId)
    {
        bool exists = await _context.CustomerWallets
            .AnyAsync(w => w.CustomerId == customerId);

        if (!exists)
        {
            _context.CustomerWallets.Add(new CustomerWallet
            {
                CustomerId = customerId,
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }

    public async Task EnsureReferralCodeAsync(int customerId, string customerName)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null || !string.IsNullOrEmpty(customer.ReferralCode))
            return;

        customer.ReferralCode = await GenerateUniqueCodeAsync(customerName);
        await _context.SaveChangesAsync();
    }

    public async Task<WalletDto> GetWalletAsync(int customerId)
    {
        var wallet = await _context.CustomerWallets
            .AsNoTracking()
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.CustomerId == customerId);

        if (wallet == null)
            return new WalletDto { Balance = 0, Transactions = new() };

        return new WalletDto
        {
            Balance = wallet.Balance,
            Transactions = wallet.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new WalletTransactionDto
                {
                    Amount = t.Amount,
                    Type = t.Type.ToString(),
                    Description = t.Description,
                    CreatedAt = t.CreatedAt
                })
                .ToList()
        };
    }

    public async Task<ReferralInfoDto> GetReferralInfoAsync(int customerId)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null || string.IsNullOrEmpty(customer.ReferralCode))
            return new ReferralInfoDto
            {
                ReferralCode = "—",
                TotalReferrals = 0,
                MaxReferrals = ReferralConstants.MaxReferrals,
                Referees = new()
            };

        var referees = await _context.ReferralUses
            .AsNoTracking()
            .Include(r => r.Referee)
            .Where(r => r.ReferrerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RefereeStatusDto
            {
                Name = r.Referee.Name,
                Status = r.Status.ToString(),
                JoinedAt = r.CreatedAt
            })
            .ToListAsync();

        return new ReferralInfoDto
        {
            ReferralCode = customer.ReferralCode,
            TotalReferrals = customer.ReferralUseCount,
            MaxReferrals = ReferralConstants.MaxReferrals,
            Referees = referees
        };
    }

    private async Task<string> GenerateUniqueCodeAsync(string name)
    {
        string prefix = new string(
            name.Where(char.IsLetter).Take(4).ToArray()
        ).ToUpper().PadRight(4, 'X');

        string code;
        bool exists;
        var rng = new Random();

        do
        {
            string suffix = rng.Next(100000, 999999).ToString();
            code = prefix + suffix;
            exists = await _context.Customers.AnyAsync(c => c.ReferralCode == code);
        }
        while (exists);

        return code;
    }

    public async Task<(bool isValid, string? errorMessage)> ValidateReferralCodeAsync(
    string referralCode)
    {
        if (string.IsNullOrWhiteSpace(referralCode))
            return (false, "Referral code is required.");

        var code = referralCode.Trim().ToUpper();

        var referrer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ReferralCode == code);

        if (referrer == null)
            return (false, "Invalid referral code. Please check and try again.");

        if (referrer.ReferralUseCount >= ReferralConstants.MaxReferrals)
            return (false, "This referral code has reached its maximum usage limit.");

        return (true, null);
    }
}