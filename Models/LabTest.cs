using System.ComponentModel.DataAnnotations;

namespace MedicalRecordService.Models;

public enum LabTestStatus
{
    Requested = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public class LabTest
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MedicalRecordId { get; set; }
    public MedicalRecord MedicalRecord { get; set; } = null!;

    public string TestName { get; set; } = null!;
    public string? TestCode { get; set; }
    public string? Result { get; set; }
    public string? NormalRange { get; set; }
    public string? Unit { get; set; }
    public LabTestStatus Status { get; set; } = LabTestStatus.Requested;
    public string? Notes { get; set; }
    public string? AttachmentFileJson { get; set; }
    public string? PerformedBy { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
