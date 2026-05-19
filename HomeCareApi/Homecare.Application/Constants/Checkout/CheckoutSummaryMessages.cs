using System;

namespace Homecare.Application.Constants.Checkout;

public class CheckoutSummaryMessages
{

    public static class Service
    {
        public const string NotFound = "Service not found.";
    }

    public static class Coupon
    {
        public const string NotFound = "Coupon not found or inactive.";
        public const string NotApplicable = "Coupon is not applicable for this booking.";
        public const string Applied = "Coupon applied successfully.";
        public const string LoadSuccess = "Available coupons fetched.";
        public const string Removed = "Coupon removed.";
    }

    public static class Summary
    {
        public const string Loaded = "Summary fetched.";
        public const string Failed = "Failed to load summary.";

    }
}
