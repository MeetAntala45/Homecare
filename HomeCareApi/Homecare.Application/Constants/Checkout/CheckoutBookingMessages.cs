namespace Homecare.Application.Constants.Checkout;

public static class CheckoutBookingMessages
{

    public const string BookingCreated = "Booking created successfully";
    public const string BookingCreatedCash = "Booking confirmed. Payment will be collected on arrival";
    public const string BookingCreatedCard = "Booking created. Proceed to payment";

    public const string SlotUpdated = "Slot updated successfully";
    public const string SlotNotChanged = "New slot is the same as the current slot";

    public const string BookingNotFound = "Booking not found";
    public const string ServiceNotFound = "Service not found";
    public const string AddressNotFound = "Address not found or does not belong to this customer";
    public const string SlotUnavailable = "Selected slot is no longer available. Please choose another slot";
    public const string NoPartnersAvailable = "No partners available for this service";
    public const string SlotInPast = "Cannot book a slot in the past";
    public const string BookingConflict = "This slot was just booked by someone else. Please choose another time.";
    public const string CannotEditCancelledBooking = "Cannot edit a cancelled booking";

    public const string PaymentSuccess = "Payment recorded successfully";
    public const string PaymentFailed = "Payment failed";
    public const string PaymentAlreadyDone = "Payment has already been processed for this booking";
    public const string SlotAlreadyBooked = "This slot is just booked by someone else.";
    public const string SlotAlreadyBookedByYou = "Your booking already exists for this same slot at same address.";
    public const string CouponNotFound = "Coupon is not found or inactive";
    public const string CouponNotApplicable = "Coupon is not applicable for this booking.";

    public const string BookingCountSuccess = "Booking count fetched successfully";
    public const string BookingCountFail = "Failed to fetch booking count";
}