using System.ComponentModel.DataAnnotations;

namespace MedicalRecordService.Models;

public class MedicalRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public Guid DoctorId { get; set; } // Lấy từ JWT Token của bác sĩ
    public string Symptoms { get; set; } = null!; // Triệu chứng
    public string Diagnosis { get; set; } = null!; // Chẩn đoán
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 1 Bệnh án có 1 Đơn thuốc
    public Prescription? Prescription { get; set; }
}