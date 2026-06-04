namespace MedicalRecordService.DTOs;

public class CreatePrescriptionDetailDto
{
    public Guid MedicationId { get; set; } // Mốt lấy cái ID này từ API của bọn Pharmacy
    public string MedicationName { get; set; } = null!;
    public int Quantity { get; set; }
    public string Dosage { get; set; } = null!; // Ví dụ: "Sáng 1 viên, tối 1 viên"
}

public class CreatePrescriptionDto
{
    public Guid MedicalRecordId { get; set; }
    public string? Instructions { get; set; } // Lời dặn dò, ví dụ: "Kiêng ăn mặn"
    
    // Một đơn phải có danh sách các loại thuốc
    public List<CreatePrescriptionDetailDto> Details { get; set; } = new List<CreatePrescriptionDetailDto>();
}