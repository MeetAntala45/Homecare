using System.ComponentModel.DataAnnotations;
using Homecare.Application.Constants;
using Homecare.Application.Constants.MasterData;

namespace Homecare.API.Model;

public class ServiceTypeCreateRequest
{
    public int Id {get; set;}

    [Required(ErrorMessage = ServiceTypeConstant.NameRequiredResponse)]
    [StringLength(50, ErrorMessage = BaseConstant.NameLengthException)]
    public string Name { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }
}
