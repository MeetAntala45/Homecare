using Homecare.Application.DTOs.MasterData;

public interface ICategory
{
    Task<IList<CategoryDto>> GetAllCategory(int serviceTypeId);

    Task AddCategory(CategoryDto dto);

    Task UpdateCategory(int id, CategoryDto dto);

    Task DeleteCategory(int id);
}