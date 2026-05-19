using Homecare.Application.DTOs.MasterData;

public interface IServiceType
{
    Task<IList<ServiceTypeDto>> GetAllServiceTypes();
    Task AddServiceType(ServiceTypeDto dto);
    Task UpdateServiceType(int id, ServiceTypeDto dto);
    Task DeleteServiceType(int id);
}