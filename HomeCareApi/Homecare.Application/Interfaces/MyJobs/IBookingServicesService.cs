namespace Homecare.Application.Interfaces.MyJobs;

public interface IBookingServicesService
{
    Task<List<string>> GetDistinctServiceNamesAsync(int partnerId);

}
