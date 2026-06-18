using System.ComponentModel.DataAnnotations;

namespace MedicalRecordService.Models;

public enum TreatmentPlanStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2
}

public class TreatmentPlan
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MedicalRecordId { get; set; }
    public MedicalRecord MedicalRecord { get; set; } = null!;

    public string PlanName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public TreatmentPlanStatus Status { get; set; } = TreatmentPlanStatus.Active;
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<TreatmentProgression> Progressions { get; set; } = new List<TreatmentProgression>();
}
