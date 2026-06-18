using System.ComponentModel.DataAnnotations;

namespace MedicalRecordService.Models;

public enum RecordStatus
{
    Draft = 0,
    Active = 1,
    Completed = 2,
    Closed = 3,
    Locked = 4
}

public class MedicalRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid DoctorId { get; set; }
    public string? Title { get; set; }
    public string Symptoms { get; set; } = null!;
    public string Diagnosis { get; set; } = null!;

    // Multi-level diagnosis
    public string? PreliminaryDiagnosis { get; set; }
    public string? FinalDiagnosis { get; set; }

    // Medical history & allergies snapshot (at time of examination)
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Status & lifecycle
    public RecordStatus Status { get; set; } = RecordStatus.Active;

    // Soft delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Lock
    public bool IsLocked { get; set; } = false;
    public DateTime? LockedAt { get; set; }
    public string? LockedBy { get; set; }

    // Audit trail
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Prescription? Prescription { get; set; }

    // Navigation
    public ICollection<LabTest> LabTests { get; set; } = new List<LabTest>();
    public ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();
}
