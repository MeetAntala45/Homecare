namespace Homecare.Application.Constants.MasterData;

public class SubCategoryConstant
{
    public const string NotFoundException = "SubCategory not found.";
    public const string AlreadyExistsException = "SubCategory already exists.";
    public const string NameRequiredResponse = "SubCategory Name is required.";
    public const string GetAllSubCategoryResponse = "SubCategory Type fetched successfully";
    public const string AddSubCategoryResponse = "SubCategory Added Successfully";
    public const string DeleteSubCategoryResponse = "SubCategory Deleted Successfully";
    public const string UpdateSubCategoryResponse = "SubCategory Detail Updated";
    public const string InvalidCategoryResponse = "Select Category first";
    public const string SubCategoryHaveServices ="Cannot delete subcategory because it has services.";

}
