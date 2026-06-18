using System.ComponentModel.DataAnnotations;

namespace MedicalRecordService.Models;

public class AuditLogEntry
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecordId { get; set; }
    public string? Field { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
