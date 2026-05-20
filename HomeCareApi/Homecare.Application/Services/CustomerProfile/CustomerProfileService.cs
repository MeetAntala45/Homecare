using System.Net.Http.Json;
using System.Security.Cryptography;
using Homecare.Application.Constants;
using Homecare.Application.Constants.CustomerProfile;
using Homecare.Application.DTOs.CustomerProfile;
using Homecare.Application.Interfaces.Auth;
using Homecare.Application.Interfaces.CustomerProfile;
using Homecare.Application.Interfaces.Referral;
using Homecare.Data;
using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Homecare.Application.Services.CustomerProfile;

public class CustomerProfileService : ICustomerProfileService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly EmailTemplateService _templateService;
    private readonly IReferralService _referralService;


    public CustomerProfileService(AppDbContext context, IEmailService emailService, HttpClient httpClient, IConfiguration config, EmailTemplateService templateService, IReferralService referralService)

    {
        _context = context;
        _emailService = emailService;
        _httpClient = httpClient;
        _config = config;
        _templateService = templateService;
        _referralService = referralService;

    }

    public async Task<ApiResponse<CustomerProfileDto>> GetProfileAsync(int customerId)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == customerId);

        var walletBalance = 0m;
        var wallet = await _context.CustomerWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.CustomerId == customerId);

        if (wallet != null)
            walletBalance = wallet.Balance;

        if (customer == null)
            return ApiResponse<CustomerProfileDto>.Fail(CustomerProfileMessages.CustomerNotFound);

        var dto = new CustomerProfileDto
        {
            Email = customer.Email,
            MobileNumber = customer.MobileNumber,
            Addresses = customer.Addresses.Select(a => new CustomerAddressDto
            {
                Id = a.Id,
                HouseFlatNo = a.HouseFlatNo,
                Landmark = a.Landmark,
                Label = a.Label,
                DisplayName = a.DisplayName,
                Latitude = a.Latitude,
                Longitude = a.Longitude
            }).ToList(),
            ReferralCode = customer.ReferralCode,
            WalletBalance = walletBalance
        };

        return ApiResponse<CustomerProfileDto>.SuccessResponse(CustomerProfileMessages.ProfileFetched, dto);
    }

    public async Task<ApiResponse<string>> UpdateMobileAsync(int customerId, UpdateMobileDto dto)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            return ApiResponse<string>.Fail(CustomerProfileMessages.CustomerNotFound);

        customer.MobileNumber = dto.MobileNumber;
        customer.ModifiedAt = DateTime.UtcNow;
        customer.ModifiedBy = customerId;

        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(CustomerProfileMessages.MobileUpdated);
    }

    public async Task<ApiResponse<EmailChangeOtpResponseDto>> RequestEmailChangeAsync(int customerId, RequestEmailChangeDto dto)
    {
        var normalizedNewEmail = dto.NewEmail.ToLower();

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            return ApiResponse<EmailChangeOtpResponseDto>.Fail(CustomerProfileMessages.CustomerNotFound);

        if (customer.Email == normalizedNewEmail)
            return ApiResponse<EmailChangeOtpResponseDto>.Fail(CustomerProfileMessages.EmailSameAsCurrent);

        var emailExists = await _context.Customers
            .AnyAsync(c => c.Email == normalizedNewEmail && c.Id != customerId);

        if (emailExists)
            return ApiResponse<EmailChangeOtpResponseDto>.Fail(CustomerProfileMessages.EmailAlreadyRegistered);

        var existingOtps = await _context.OtpVerifications
            .Where(o => o.Email == normalizedNewEmail && !o.IsUsed && !o.IsRevoked)
            .ToListAsync();

        existingOtps.ForEach(o => o.IsRevoked = true);

        var otpCode = GenerateOtp();

        var otp = new OtpVerification
        {
            CustomerId = customerId,
            Email = normalizedNewEmail,
            OtpCode = otpCode,
            IsUsed = false,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        _context.OtpVerifications.Add(otp);
        await _context.SaveChangesAsync();

        var htmlBody = _templateService.GetTemplate(
            "EmailChangeOtpRequest.html",
            new Dictionary<string, string>
            {
                { "Name",  customer.Name ?? "User" },
                { "OtpCode", otpCode }
            }
        );

        await _emailService.SendAsync(
            normalizedNewEmail,
            CustomerProfileMessages.EmailOtpSubject,
            htmlBody
        );

        return ApiResponse<EmailChangeOtpResponseDto>.SuccessResponse(
            CustomerProfileMessages.EmailOtpSent,
            new EmailChangeOtpResponseDto
            {
                Message = CustomerProfileMessages.EmailOtpSent,
                ExpiresAt = otp.ExpiresAt
            }
        );
    }

    public async Task<ApiResponse<string>> VerifyEmailChangeAsync(int customerId, VerifyEmailChangeDto dto)
    {
        var normalizedNewEmail = dto.NewEmail.ToLower();

        var otpRecord = await _context.OtpVerifications
            .Where(o => o.Email == normalizedNewEmail
                     && o.IsUsed == false
                     && o.IsRevoked == false
                     && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otpRecord == null)
            return ApiResponse<string>.Fail(CustomerProfileMessages.EmailOtpExpiredOrInvalid);

        if (otpRecord.OtpCode != dto.Otp)
            return ApiResponse<string>.Fail(CustomerProfileMessages.EmailOtpIncorrect);

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            return ApiResponse<string>.Fail(CustomerProfileMessages.CustomerNotFound);

        customer.Email = normalizedNewEmail;
        customer.ModifiedAt = DateTime.UtcNow;
        customer.ModifiedBy = customerId;

        otpRecord.IsUsed = true;

        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(CustomerProfileMessages.EmailUpdated);
    }

    public async Task<ApiResponse<string>> AddAddressAsync(int customerId, AddressRequestDto dto)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            return ApiResponse<string>.Fail(CustomerProfileMessages.CustomerNotFound);

        if (dto.Latitude.HasValue && dto.Longitude.HasValue)
        {
            var isDuplicateAddress = await _context.Addresses.AnyAsync(a =>
                a.CustomerId == customerId &&
                a.Latitude == dto.Latitude &&
                a.Longitude == dto.Longitude &&
                a.HouseFlatNo.Trim().ToLower() == dto.HouseFlatNo.Trim().ToLower() &&
                a.Landmark.Trim().ToLower() == dto.Landmark.Trim().ToLower());

            if (isDuplicateAddress)
            {
                return ApiResponse<string>.Fail(CustomerProfileMessages.AddressAlreadyExists);
            }
        }

        var normalizedLabel = System.Globalization.CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(dto.Label.Trim().ToLower());

        string city = "Unknown";

        if (dto.Latitude.HasValue && dto.Longitude.HasValue)
        {
            city = await GetCityAsync(
                dto.Latitude.Value,
                dto.Longitude.Value
            ) ?? "Unknown";
        }

        var address = new Address
        {
            CustomerId = customerId,
            HouseFlatNo = dto.HouseFlatNo.Trim(),
            Landmark = dto.Landmark.Trim(),
            Label = normalizedLabel.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            DisplayName = dto.DisplayName,
            CreatedAt = DateTime.UtcNow,
            City = city
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(CustomerProfileMessages.AddressAdded);
    }

    public async Task<ApiResponse<string>> EditAddressAsync(int customerId, int addressId, AddressRequestDto dto)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId);

        if (address == null)
            return ApiResponse<string>.Fail(CustomerProfileMessages.AddressNotFound);

        var normalizedLabel = System.Globalization.CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(dto.Label.Trim().ToLower());

        string city = "Unknown";

        if (dto.Latitude.HasValue && dto.Longitude.HasValue)
        {
            city = await GetCityAsync(
                dto.Latitude.Value,
                dto.Longitude.Value
            ) ?? "Unknown";
        }

        address.DisplayName = dto.DisplayName;
        address.HouseFlatNo = dto.HouseFlatNo.Trim();
        address.Landmark = dto.Landmark.Trim();
        address.Label = normalizedLabel.Trim();
        address.Latitude = dto.Latitude;
        address.Longitude = dto.Longitude;
        address.ModifiedAt = DateTime.UtcNow;
        address.City = city;

        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(CustomerProfileMessages.AddressUpdated);
    }

    public async Task<ApiResponse<string>> DeleteAddressAsync(int customerId, int addressId)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId);

        if (address == null)
            return ApiResponse<string>.Fail(CustomerProfileMessages.AddressNotFound);

        bool isUsedInBooking = await _context.Bookings
            .IgnoreQueryFilters()
            .AnyAsync(b => b.AddressId == addressId);

        if (isUsedInBooking)
            return ApiResponse<string>.Fail(CustomerProfileMessages.AddressDeletionNotPermitted);

        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(CustomerProfileMessages.AddressDeleted);
    }

    public async Task<ApiResponse<List<string>>> GetAddressLabelsAsync(int customerId)
    {
        var customLabels = await _context.Addresses
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId && a.Label != "Home")
            .Select(a => a.Label)
            .Distinct()
            .ToListAsync();

        var labels = new List<string> { "Home" };
        labels.AddRange(customLabels);

        return ApiResponse<List<string>>.SuccessResponse(CustomerProfileMessages.LabelsFetched, labels);
    }

    public async Task<ApiResponse<string>> AddRecentSearchAsync(int customerId, AddRecentSearchDto dto)
    {
        var existing = await _context.RecentSearches
            .Where(r => r.CustomerId == customerId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        var distinct = existing.DistinctBy(r => r.DisplayName).ToList();

        if (distinct.Count >= 3)
        {
            _context.RecentSearches.Remove(existing.First());
        }

        var recentSearch = new RecentSearch
        {
            CustomerId = customerId,
            DisplayName = dto.DisplayName.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            CreatedAt = DateTime.UtcNow
        };

        _context.RecentSearches.Add(recentSearch);
        await _context.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(CustomerProfileMessages.RecentSearchSaved);
    }

    public async Task<ApiResponse<List<RecentSearchDto>>> GetRecentSearchesAsync(int customerId)
    {
        var searches = await _context.RecentSearches
            .AsNoTracking()
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var distinct = searches.DistinctBy(r => r.DisplayName).Take(3).ToList();

        var result = distinct.Select(r =>
        {
            var parts = r.DisplayName.Split(',').Select(p => p.Trim()).ToList();

            var title = parts.Count >= 2
                ? string.Join(", ", parts.Take(2))
                : parts[0];

            var description = parts.Count > 2
                ? string.Join(", ", parts.Skip(2))
                : string.Empty;

            return new RecentSearchDto
            {
                Id = r.Id,
                DisplayName = r.DisplayName,
                Title = title,
                Description = description,
                Latitude = r.Latitude,
                Longitude = r.Longitude
            };
        }).ToList();

        return ApiResponse<List<RecentSearchDto>>.SuccessResponse(CustomerProfileMessages.RecentSearchesFetched, result);
    }

    private static string GenerateOtp()
    {
        return RandomNumberGenerator.GetInt32(0, 9999).ToString("D4");
    }

    public async Task<string?> GetCityAsync(decimal lat, decimal lng)
    {
        var apiKey = _config["Geo:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("Geo API Key is missing");

        var url = $"https://api.opencagedata.com/geocode/v1/json?q={lat}+{lng}&key={apiKey}";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var data = await response.Content.ReadFromJsonAsync<OpenCageResponseDto>();

        var components = data?.results?.FirstOrDefault()?.components;

        if (components == null)
            return null;

        var city = new[]
        {
        components.city,
        components.district,
        components.municipality,
        components.town,
        components.village,
        components.hamlet,
        components.neighbourhood,
        components.city_district,
        components.state_district,
        components.county,
        components.state,
        components.region,
        components.province
    }
        .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

        return city?.Trim();
    }
}