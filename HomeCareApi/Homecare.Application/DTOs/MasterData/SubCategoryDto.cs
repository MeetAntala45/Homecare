using System.ComponentModel.DataAnnotations;
using Homecare.Application.Constants;
using Homecare.Application.Constants.MasterData;

namespace Homecare.Application.DTOs.MasterData;

public class SubCategoryDto
{
    public int Id { get; set; }
    [Required(ErrorMessage = SubCategoryConstant.NameRequiredResponse)]
    [StringLength(50, ErrorMessage = BaseConstant.NameLengthException)]
    public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "Category is required.")]

    public int CategoryId { get; set; }
}
