using System.ComponentModel.DataAnnotations;

namespace MedicalRecordService.Models;

public class Prescription
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MedicalRecordId { get; set; }
    public MedicalRecord MedicalRecord { get; set; } = null!;

    public string? Instructions { get; set; }
    public DateTime PrescribedAt { get; set; } = DateTime.UtcNow;

    // Trạng thái đơn thuốc
    public string Status { get; set; } = "active"; // active, completed, cancelled, expired
    public DateTime? ExpiryDate { get; set; }
    public int RefillCount { get; set; } = 0;

    // Audit
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public ICollection<PrescriptionDetail> Details { get; set; } = new List<PrescriptionDetail>();
}
