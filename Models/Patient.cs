using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalRecordService.Models;

public class Patient
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();
    public int? GatewayPatientId { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = null!;
    public string? MedicalHistory { get; set; }
    public string? Allergies { get; set; }
    public string? BloodGroup { get; set; }

    // Mở rộng thông tin
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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}
