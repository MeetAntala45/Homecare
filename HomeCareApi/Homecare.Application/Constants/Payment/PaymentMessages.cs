namespace Homecare.Application.Constants.Payments;

public static class PaymentMessages
{
    public const string BookingNotFound = "Booking not found.";
    public const string BookingAlreadyPaid = "This booking is already paid.";
    public const string BookingExpiredOrCancelled = "This booking has expired or been cancelled. Please create a new booking.";
    public const string BookingSuccessFetched = "Booking success details fetched.";
    public const string PaymentNotFound = "Payment record not found.";
    public const string PaymentNotFoundForBooking = "Payment record not found for this booking.";
    public const string PaymentNotCompleted = "Payment not completed for this booking.";
    public const string CheckoutSessionCreated = "Checkout session created successfully.";
    public const string WebhookHandled = "Webhook handled.";
    public const string InvoiceNotGenerated = "Invoice file could not be generated. Please try again.";
    public const string ServiceNotFound = "Service not found.";
    public const string NoExpiredBookings = "No expired bookings found.";
    public const string PaymentExpired = "expired";
    public const string PaymentRefunded = "refunded";
    public const string RefundInitiated = "Refund initiated successfully.";
    public const string RefundNotNeeded = "No refund needed for this payment method.";
    public const string RefundAlreadyProcessed = "Refund already processed.";
}