using Homecare.Application.DTOs.MasterData;

public interface ISubCategory
{
    Task<IList<SubCategoryDto>> GetAllSubCategory(int categoryId);

    Task AddSubCategory(SubCategoryDto dto);

    Task UpdateSubCategory(int id, SubCategoryDto dto);

    Task DeleteSubCategory(int id);
}