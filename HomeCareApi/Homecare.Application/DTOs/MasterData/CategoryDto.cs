using System.ComponentModel.DataAnnotations;
using Homecare.Application.Constants;
using Homecare.Application.Constants.MasterData;

namespace Homecare.Application.DTOs.MasterData;

public class CategoryDto
{
    public int Id { get; set; }
    [Required(ErrorMessage = CategoryConstant.NameRequiredResponse)]
    [StringLength(50, ErrorMessage = BaseConstant.NameLengthException)]
    public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "Service Type is required.")]
    public int ServiceTypeId { get; set; }
}
