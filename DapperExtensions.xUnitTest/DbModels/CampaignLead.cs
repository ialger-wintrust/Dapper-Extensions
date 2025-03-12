using System.ComponentModel.DataAnnotations;

namespace DapperExtensions.xUnitTest.DbModels;

public class CampaignLead
{
    [Key]
    public int Id { get; set; }

    public int ActiniumId { get; set; }

    [Required]
    [StringLength(11)]
    public string PhoneNumber { get; set; }

    [StringLength(11)]
    public string OutboundAni { get; set; }

    [Required]
    [StringLength(50)]
    public string CampaignName { get; set; }

    [StringLength(255)]
    public string CampaignSource { get; set; }

    public byte? ContactAttempt { get; set; }
}