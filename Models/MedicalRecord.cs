using System.ComponentModel.DataAnnotations;

namespace MedicalRecordService.Models;

public class MedicalRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public Guid DoctorId { get; set; } // Lấy từ JWT Token của bác sĩ
    public string? Title { get; set; } // Tiêu đề bệnh án
    public string Symptoms { get; set; } = null!; // Triệu chứng
    public string Diagnosis { get; set; } = null!; // Chẩn đoán
    public string? Notes { get; set; }
    public double? Weight { get; set; } // Cân nặng (kg)
    public double? Height { get; set; } // Chiều cao (cm)
    public string? BloodPressure { get; set; } // Huyết áp (mmHg)
    public int? HeartRate { get; set; } // Nhịp tim (bpm)
    public double? Temperature { get; set; } // Nhiệt độ (°C)
    public string? CustomMetricsJson { get; set; } // Bảng chỉ số dịch vụ khám
    public string? AttachmentsJson { get; set; } // Đính kèm tệp
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 1 Bệnh án có 1 Đơn thuốc
    public Prescription? Prescription { get; set; }
}