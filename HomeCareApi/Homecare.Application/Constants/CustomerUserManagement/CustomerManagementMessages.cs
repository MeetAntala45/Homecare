namespace Homecare.Application.Constants.CustomerUserManagement;

public static class CustomerManagementMessages
{
    public const string CustomerDoesNotExist = "Customer doesn't exist";
    public const string InvalidData = "Invalid data.";
    public const string CustomersFetched = "Customers fetched successfully.";
    public const string CustomerDetailsFetched = "Customer details fetched";
    public const string EmailAlreadyRegistered = "This email is already registered with another account.";
    public const string CustomerAdded = "Customer added successfully.";
    public const string CustomerDeleted = "Customer deleted successfully.";
    public const string AlreadyBlocked = "Customer is already blocked.";
    public const string NotBlocked = "Customer is not blocked.";
    public const string CustomerBlocked = "Customer blocked successfully.";
    public const string CustomerUnblocked = "Customer unblocked successfully.";
    public const string NotInactive = "Customer is not inactive.";
    public const string CustomerActivated = "Customer has been activated successfully.";
    public const string BookingNotFound = "Booking not found.";
    public const string BookingsFetched = "Bookings fetched successfully.";
    public const string BookingCompleted = "Booking completed successfully.";
    public const string BookingAlreadyCompleted = "Booking is already completed.";
    public const string BookingCompletedNoDelete = "Booking is already completed. So it can't be deleted";
    public const string BookingCancelled = "Booking cancelled successfully.";
    public const string BookingAlreadyCancelled = "Booking is already cancelled.";
    public const string BookingDeleted = "Booking deleted successfully.";
    public const string CannotCompleteBooking = "Cannot complete a cancelled booking.";
    public const string CannotCancelBooking = "Cannot cancel a completed booking.";
    public const string CannotChangeExpert = "Cannot change expert for completed or cancelled bookings.";

    public const string CannotCancleInProgressBooking = "Cannot cancle a InProgressed Booking";
    public const string AvailablePartnersFetched = "Available partners fetched successfully.";
    public const string ExpertChanged = "Expert changed successfully.";
    public const string PartnerNotFound = "Selected partner not found.";
    public const string ServiceNotFound = "Service not found.";
    public const string NoAvailablePartners = "No available partners for this time slot.";
}