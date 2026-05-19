namespace Homecare.Application.DTOs.AdminUser;

public class AdminListItemDto
{
    public int Id {get; set;}
    public string Name {get; set;} = null!;
    public string Email {get; set;} = null!;
    public string MobileNumber {get; set;} = null!;
    public string Role {get; set;} = null!;
    public bool IsActive {get; set;}
}

