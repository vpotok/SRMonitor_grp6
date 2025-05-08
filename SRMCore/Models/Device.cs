using System.ComponentModel.DataAnnotations;

namespace SRMCore.Models;

public class Device
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    public bool IsActive { get; set; } = true;
}