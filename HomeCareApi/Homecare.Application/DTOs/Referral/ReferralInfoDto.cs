namespace Homecare.Application.DTOs.Referral;

public class ReferralInfoDto
{
    public string  ReferralCode    { get; set; } = null!;
    public int     TotalReferrals  { get; set; }
    public int     MaxReferrals    { get; set; }
    public List<RefereeStatusDto> Referees { get; set; } = new();
}

public class RefereeStatusDto
{
    public string  Name    { get; set; } = null!;
    public string  Status  { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
}