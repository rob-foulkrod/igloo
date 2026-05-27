using System.ComponentModel.DataAnnotations;

namespace web.Models;

public class Registration
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "EventId is required.")]
    [Display(Name = "Event")]
    public int EventId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Registered At")]
    public DateTime RegisteredAt { get; set; }
}
