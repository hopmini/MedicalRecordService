using System.ComponentModel.DataAnnotations;

namespace MedicalRecordService.Models;

public class PrescriptionDetail
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PrescriptionId { get; set; }
    public Prescription Prescription { get; set; } = null!;
    
    public Guid MedicationId { get; set; } // ID thuốc (sau gọi qua Pharmacy Service)
    public string MedicationName { get; set; } = null!; // Lưu sẵn tên cho dễ tra cứu
    public int Quantity { get; set; }
    public string Dosage { get; set; } = null!; // Liều dùng (VD: "2 viên/ngày")
}