using Homecare.Application.DTOs.Checkout;

namespace Homecare.Application.Interfaces.Checkout;

public interface ISlotService
{
    Task<List<SlotResponseDto>> GetSlots(GetSlotsRequestDto dto, int customerId);
    Task<List<DateAvailabilityDto>> GetAvailableDates(int serviceId, int customerId);   
    Task<Dictionary<int, bool>> GetServicePartnerAvailability();
}
