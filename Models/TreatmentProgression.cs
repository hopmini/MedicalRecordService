using System.ComponentModel.DataAnnotations;

namespace MedicalRecordService.Models;

public class TreatmentProgression
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TreatmentPlanId { get; set; }
    public TreatmentPlan TreatmentPlan { get; set; } = null!;

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = null!;
    public string? Status { get; set; }
    public string? RecordedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
