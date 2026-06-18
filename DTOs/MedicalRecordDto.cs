namespace MedicalRecordService.DTOs;

public class CreateMedicalRecordDto
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public string? Title { get; set; }
    public string Symptoms { get; set; } = null!;
    public string Diagnosis { get; set; } = null!;

    public string? PreliminaryDiagnosis { get; set; }
    public string? FinalDiagnosis { get; set; }
    public string? MedicalHistorySnapshot { get; set; }
    public string? AllergiesSnapshot { get; set; }

    // ICD-10
    public string? DiagnosisCode { get; set; }
    public string? DiagnosisCodeName { get; set; }

    // Discharge Summary
    public DateTime? AdmissionDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public string? DischargeDiagnosis { get; set; }
    public string? DischargeInstructions { get; set; }
    public string? FollowUpInstructions { get; set; }
    public string? FollowUpClinic { get; set; }
    public DateTime? FollowUpDate { get; set; }

    public string? Notes { get; set; }
    public double? Weight { get; set; }
    public double? Height { get; set; }
    public string? BloodPressure { get; set; }
    public int? HeartRate { get; set; }
    public double? Temperature { get; set; }
    public string? CustomMetricsJson { get; set; }
    public string? AttachmentsJson { get; set; }

    public Guid? AppointmentId { get; set; }
    public string? PatientName { get; set; }
    public int? GatewayPatientId { get; set; }
}

public class UpdateMedicalRecordDto
{
    public string? Title { get; set; }
    public string? Symptoms { get; set; }
    public string? Diagnosis { get; set; }
    public string? PreliminaryDiagnosis { get; set; }
    public string? FinalDiagnosis { get; set; }
    public string? MedicalHistorySnapshot { get; set; }
    public string? AllergiesSnapshot { get; set; }
    public string? DiagnosisCode { get; set; }
    public string? DiagnosisCodeName { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public string? DischargeDiagnosis { get; set; }
    public string? DischargeInstructions { get; set; }
    public string? FollowUpInstructions { get; set; }
    public string? FollowUpClinic { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public string? Notes { get; set; }
    public double? Weight { get; set; }
    public double? Height { get; set; }
    public string? BloodPressure { get; set; }
    public int? HeartRate { get; set; }
    public double? Temperature { get; set; }
    public string? CustomMetricsJson { get; set; }
    public string? AttachmentsJson { get; set; }
}

public class MedicalRecordResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public int? GatewayPatientId { get; set; }
    public Guid DoctorId { get; set; }
    public string? Title { get; set; }
    public string Symptoms { get; set; } = null!;
    public string Diagnosis { get; set; } = null!;
    public string? PreliminaryDiagnosis { get; set; }
    public string? FinalDiagnosis { get; set; }
    public string? MedicalHistorySnapshot { get; set; }
    public string? AllergiesSnapshot { get; set; }
    public string? DiagnosisCode { get; set; }
    public string? DiagnosisCodeName { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public string? DischargeDiagnosis { get; set; }
    public string? DischargeInstructions { get; set; }
    public string? FollowUpInstructions { get; set; }
    public string? FollowUpClinic { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public string? Notes { get; set; }
    public double? Weight { get; set; }
    public double? Height { get; set; }
    public string? BloodPressure { get; set; }
    public int? HeartRate { get; set; }
    public double? Temperature { get; set; }
    public string? CustomMetricsJson { get; set; }
    public string? AttachmentsJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsDeleted { get; set; }
    public bool IsLocked { get; set; }
    public string? LockedBy { get; set; }
    public PrescriptionResponseDto? Prescription { get; set; }
    public List<LabTestResponseDto>? LabTests { get; set; }
    public List<TreatmentPlanResponseDto>? TreatmentPlans { get; set; }
}

// LabTest DTOs
public class CreateLabTestDto
{
    public string TestName { get; set; } = null!;
    public string? TestCode { get; set; }
    public string? NormalRange { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
}

public class UpdateLabTestDto
{
    public string? Result { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentFileJson { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class LabTestResponseDto
{
    public Guid Id { get; set; }
    public Guid MedicalRecordId { get; set; }
    public string TestName { get; set; } = null!;
    public string? TestCode { get; set; }
    public string? Result { get; set; }
    public string? NormalRange { get; set; }
    public string? Unit { get; set; }
    public string Status { get; set; } = "Requested";
    public string? Notes { get; set; }
    public string? AttachmentFileJson { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

// TreatmentPlan DTOs
public class CreateTreatmentPlanDto
{
    public string PlanName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UpdateTreatmentPlanDto
{
    public string? PlanName { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
}

public class TreatmentPlanResponseDto
{
    public Guid Id { get; set; }
    public Guid MedicalRecordId { get; set; }
    public string PlanName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TreatmentProgressionResponseDto>? Progressions { get; set; }
}

public class CreateProgressionDto
{
    public string Notes { get; set; } = null!;
    public string? Status { get; set; }
}

public class TreatmentProgressionResponseDto
{
    public Guid Id { get; set; }
    public DateTime RecordedAt { get; set; }
    public string Notes { get; set; } = null!;
    public string? Status { get; set; }
    public string? RecordedBy { get; set; }
}

// Audit DTOs
public class AuditLogEntryResponseDto
{
    public Guid Id { get; set; }
    public Guid RecordId { get; set; }
    public string? Field { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
}

// Status update DTO
public class UpdateStatusDto
{
    public string Status { get; set; } = null!;
}

// Prescription DTOs (unchanged)
public class PrescriptionResponseDto
{
    public Guid Id { get; set; }
    public string? Instructions { get; set; }
    public DateTime PrescribedAt { get; set; }
    public string Status { get; set; } = "active";
    public DateTime? ExpiryDate { get; set; }
    public int RefillCount { get; set; }
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

public class UpdatePrescriptionDto
{
    public string? Instructions { get; set; }
    public string? Status { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public List<CreatePrescriptionDetailDto>? Details { get; set; }
}
