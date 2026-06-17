using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalRecordService.Models;

public class Patient
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();
    public int? GatewayPatientId { get; set; } // int ID từ Gateway (IdentityDB)
    public string FullName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = null!;
    public string? MedicalHistory { get; set; } // Tiền sử bệnh
    public string? Allergies { get; set; } // Dị ứng
    public string? BloodGroup { get; set; } // Nhóm máu
    
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}