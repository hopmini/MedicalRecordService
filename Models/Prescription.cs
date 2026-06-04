using System.ComponentModel.DataAnnotations;

namespace MedicalRecordService.Models;

public class Prescription
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MedicalRecordId { get; set; }
    public MedicalRecord MedicalRecord { get; set; } = null!;
    
    public string? Instructions { get; set; } // Lời dặn của bác sĩ
    public DateTime PrescribedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PrescriptionDetail> Details { get; set; } = new List<PrescriptionDetail>();
}