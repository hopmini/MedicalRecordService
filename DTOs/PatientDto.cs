namespace MedicalRecordService.DTOs;

public class CreatePatientDto
{
    public string FullName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = null!;
    public string? MedicalHistory { get; set; }
    public string? Allergies { get; set; }
    public string? BloodGroup { get; set; }
    public int? GatewayPatientId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? IdentityCard { get; set; }
    public string? InsuranceNumber { get; set; }
    public string? Occupation { get; set; }
    public string? Ethnicity { get; set; }
    public string? Nationality { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
}

public class UpdatePatientDto
{
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? MedicalHistory { get; set; }
    public string? Allergies { get; set; }
    public string? BloodGroup { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? IdentityCard { get; set; }
    public string? InsuranceNumber { get; set; }
    public string? Occupation { get; set; }
    public string? Ethnicity { get; set; }
    public string? Nationality { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
}

public class PatientResponseDto
{
    public Guid Id { get; set; }
    public int? GatewayPatientId { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = null!;
    public string? MedicalHistory { get; set; }
    public string? Allergies { get; set; }
    public string? BloodGroup { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? IdentityCard { get; set; }
    public string? InsuranceNumber { get; set; }
    public string? Occupation { get; set; }
    public string? Ethnicity { get; set; }
    public string? Nationality { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
}
