namespace Homecare.Domain.Entities;

public class PartnerDocument : BaseEntity
{
    public int Id { get; set; }

    public int PartnerId { get; set; }

    public string DocumentName { get; set; }

    public string FilePath { get; set; }

    public int FileSizeKb { get; set; }

    public string FileType { get; set; }

    public ServicePartner Partner { get; set; }
}