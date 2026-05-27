using System.ComponentModel.DataAnnotations;

namespace web.Models;

public class Event : IValidatableObject
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [Required]
    public string Location { get; set; } = string.Empty;

    [Range(1, 10000)]
    public int Capacity { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate <= DateTime.UtcNow)
            yield return new ValidationResult(
                "Start date must be in the future.",
                new[] { nameof(StartDate) });

        if (EndDate <= StartDate)
            yield return new ValidationResult(
                "End date must be after start date.",
                new[] { nameof(EndDate) });
    }
}
