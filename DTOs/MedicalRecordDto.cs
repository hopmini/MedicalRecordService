namespace MedicalRecordService.DTOs;

public class CreateMedicalRecordDto
{
    public Guid PatientId { get; set; }
    
    // Đáng lẽ cái này mốc từ JWT token của bác sĩ lúc login, 
    // nhưng giờ mình chưa nối Auth nên cứ truyền tay từ Postman/Swagger vào đã
    public Guid DoctorId { get; set; } 
    
    public string Symptoms { get; set; } = null!;
    public string Diagnosis { get; set; } = null!;
    public string? Notes { get; set; }
}

public class MedicalRecordResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public string Symptoms { get; set; } = null!;
    public string Diagnosis { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public PrescriptionResponseDto? Prescription { get; set; }
}

public class PrescriptionResponseDto
{
    public Guid Id { get; set; }
    public string? Instructions { get; set; }
    public DateTime PrescribedAt { get; set; }
    public List<PrescriptionDetailResponseDto> Details { get; set; } = new();
}

public class PrescriptionDetailResponseDto
{
    public Guid Id { get; set; }
    public Guid MedicationId { get; set; }
    public string MedicationName { get; set; } = null!;
    public int Quantity { get; set; }
    public string Dosage { get; set; } = null!;
}