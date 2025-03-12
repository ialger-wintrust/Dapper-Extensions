using System.ComponentModel.DataAnnotations;

namespace DapperExtensions.xUnitTest.DbModels;

public class UserGuid
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; }
}