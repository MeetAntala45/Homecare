using Homecare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
// dotnet ef migrations add RefactorAddressLabel --project Homecare.Data --startup-project Homecare.API
// dotnet ef database update --project Homecare.Data --startup-project Homecare.API
namespace Homecare.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Coupon> coupons { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceImage> ServiceImages { get; set; }
        public DbSet<ServiceChecklist> ServiceChecklists { get; set; }
        public DbSet<ServicePartner> ServicePartners { get; set; }
        public DbSet<PartnerEducation> PartnerEducations { get; set; }
        public DbSet<PartnerExperience> PartnerExperiences { get; set; }
        public DbSet<PartnerSkill> PartnerSkills { get; set; }
        public DbSet<PartnerServiceOffered> PartnerServicesOffered { get; set; }
        public DbSet<PartnerLanguage> PartnerLanguages { get; set; }
        public DbSet<PartnerDocument> PartnerDocuments { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }
        public DbSet<RecentSearch> RecentSearches { get; set; }
        public DbSet<Support> Supports { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<CouponCondition> CouponConditions { get; set; }
        public DbSet<CouponConditionType> CouponConditionTypes { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PartnerOtpVerification> PartnerOtpVerifications { get; set; }
        public DbSet<AdminNotification> AdminNotifications { get; set; }
        public DbSet<AdminNotificationRead> AdminNotificationReads { get; set; }

        public DbSet<PartnerNotification> PartnerNotifications { get; set; }
        public DbSet<PartnerNotificationRead> PartnerNotificationReads { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<PartnerLeave> PartnerLeaves { get; set; }
        public DbSet<PartnerSystemNotification> PartnerSystemNotifications { get; set; }
        public DbSet<AdminSystemNotification> AdminSystemNotifications { get; set; }
        public DbSet<CustomerWallet> CustomerWallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<ReferralUse> ReferralUses { get; set; }
        // In AppDbContext.cs — add:
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        // public DbSet<PartnerLocation> PartnerLocations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

    }
}