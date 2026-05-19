namespace Homecare.Application.Constants.Caching;

public static class CacheKeys
{
    public const string TopServicePartners = "dashboard_top_service_partners";

    public static string RevenueChart(string period, string? week)
        => $"dashboard_revenue_{period}_{week}";

    public static string TopCities(string period, string? week)
        => $"dashboard_top_cities_{period}_{week}";

    public static string TopServices(string period, string? week)
        => $"dashboard_top_services_{period}_{week}";

    public static string SummaryCards() => "dashboard_summary_cards";

    public static string PartnerSummaryCards(int partnerId)
        => $"partner_summary_cards_{partnerId}";

    public static string PartnerRevenueChart(int partnerId, string period, string? week)
        => $"partner_revenue_chart_{partnerId}_{period}_{week ?? "this"}";

    public static string PartnerTopServices(int partnerId, string period, string? week)
        => $"partner_top_services_{partnerId}_{period}_{week ?? "this"}";
}

