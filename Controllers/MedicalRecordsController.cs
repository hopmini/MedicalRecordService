using MedicalRecordService.Data;
using MedicalRecordService.DTOs;
using MedicalRecordService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace MedicalRecordService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public MedicalRecordsController(MedicalDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    // API: Đổ dữ liệu mẫu để test các chỉ số mới (AllowAnonymous)
    [HttpGet("seed-demo")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedDemoData()
    {
        var patientId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        var doctorId = Guid.Parse("b7201c65-934a-461c-8f0f-7bd158215e43");

        // 1. Đảm bảo bệnh nhân tồn tại trong DB
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == patientId);
        if (patient == null)
        {
            patient = new Patient
            {
                Id = patientId,
                GatewayPatientId = 3,
                FullName = "Nguyễn Văn Bệnh Nhân Demo",
                DateOfBirth = new DateTime(1995, 5, 15),
                Gender = "Nam",
                MedicalHistory = "Khỏe mạnh, chưa phát hiện dị ứng",
                Allergies = "Không"
            };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
        }

        // 2. Tạo bệnh án 1: Tầm soát tim mạch
        var record1 = new MedicalRecord
        {
            PatientId = patientId,
            DoctorId = doctorId,
            Title = "Khám Tổng Quát & Tầm Soát Tim Mạch",
            Symptoms = "Đau ngực nhẹ khi vận động mạnh, thỉnh thoảng khó thở về đêm.",
            Diagnosis = "Trào ngược dạ dày thực quản (GERD) mức độ nhẹ, theo dõi rối loạn nhịp tim chậm.",
            Notes = "Tránh thức ăn cay nóng, đồ uống có gas. Không ăn muộn sau 20h. Tập thể dục nhẹ nhàng.",
            Weight = 72.5,
            Height = 175.0,
            BloodPressure = "125/82",
            HeartRate = 68,
            Temperature = 36.6,
            CustomMetricsJson = "[{\"name\": \"Điện tâm đồ (ECG)\", \"value\": \"Nhịp xoang đều, tần số 68 ck/phút\"}, {\"name\": \"Glucose máu\", \"value\": \"5.4 mmol/L (Bình thường)\"}, {\"name\": \"Cholesterol toàn phần\", \"value\": \"4.8 mmol/L\"}]",
            AttachmentsJson = "[{\"name\": \"Ket_Qua_Sieu_Am_Tim.pdf\", \"data\": \"data:application/pdf;base64,JVBERi0xLjQKJdPr6XsKMSAwIG9iagogIDw8IC9UeXBlIC9DYXRhbG9nCiAgICAgL1BhZ2VzIDIgMCBSCiAgPj4KZW5kb2JqCjIgMCBvYmoKICA8PCAvVHlwZSAvUGFnZXMKICAgICAvS2lkcyBbIDMgMCBSIF0KICAgICAvQ291bnQgMQogID4+CmVuZG9iagozIDAgb2JqCiAgPDwgL1R5cGUgL1BhZ2UKICAgICAvUGFyZW50IDIgMCBSCiAgICAgL01lZGlhQm94IFsgMCAwIDU5NSA4NDIgXQogICAgIC9Db250ZW50cyA0IDAgUgogICAgIC9SZXNvdXJjZXMgPDwgL0ZvbnQgPDwgL0YxIDUgMCBSID4+ID4+CiAgPj4KZW5kb2JqCjQgMCBvYmoKICA8PCAvTGVuZ3RoIDY1ID4+CnN0cmVhbQpCVEQKL0YxIDEyIFRmCjcwIDcwMCBUZAooS2V0IHF1YSBzaWV1IGFtIHRpbTogY2h1Y25hbmcgdGF0IGNhIGJpbmggdGh1b25nLikgVGoKRVQKZW5kc3RyZWFtCmVuZG9iago1IDAgb2JqCiAgPDwgL1R5cGUgL0ZvbnQKICAgICAvU3VidHlwZSAvVHlwZTEKICAgICAvQmFzZUZvbnQgL0hlbHZldGljYQogID4+CmVuZG9iagp4cmVmCjAgNgowMDAwMDAwMDAwIDY1NTM1IGYgCjAwMDAwMDAwMTcgMDAwMDAgbiAKMDAwMDAwMDA4MCAwMDAwMCBuIAowMDAwMDAwMTM4IDAwMDAwIGYgCjAwMDAwMDAyNzggMDAwMDAgbiAKMDAwMDAwMDM5NCAwMDAwMCBuIAp0cmFpbGVyCiAgPDwgL1NpemUgNgogICAgIC9Sb290IDEgMCBSCiAgPj4Kc3RhcnR4cmVmCjQ5OQolJUVPRgo=\"}]",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        _context.MedicalRecords.Add(record1);
        await _context.SaveChangesAsync();

        // Kê đơn cho bệnh án 1
        var pres1 = new Prescription
        {
            MedicalRecordId = record1.Id,
            Instructions = "Uống thuốc sau bữa ăn sáng và tối. Tái khám sau 2 tuần.",
            PrescribedAt = DateTime.UtcNow.AddDays(-10),
            Details = new List<PrescriptionDetail>
            {
                new PrescriptionDetail
                {
                    MedicationId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    MedicationName = "Panadol Extra 500mg",
                    Quantity = 20,
                    Dosage = "Sáng 1 viên, Tối 1 viên"
                }
            }
        };
        _context.Prescriptions.Add(pres1);
        await _context.SaveChangesAsync();

        // 3. Tạo bệnh án 2: Tai Mũi Họng
        var record2 = new MedicalRecord
        {
            PatientId = patientId,
            DoctorId = doctorId,
            Title = "Khám Chuyên Khoa Tai Mũi Họng",
            Symptoms = "Ho khan kéo dài, đau rát họng đặc biệt vào sáng sớm, có đờm nhẹ.",
            Diagnosis = "Viêm họng hạt cấp tính, trào ngược thực quản gây kích ứng họng.",
            Notes = "Súc họng bằng nước muối sinh lý ấm 3 lần/ngày. Giữ ấm vùng cổ, uống nhiều nước ấm.",
            Weight = 73.0,
            Height = 175.0,
            BloodPressure = "120/80",
            HeartRate = 72,
            Temperature = 37.2,
            CustomMetricsJson = "[{\"name\": \"Nội soi Tai Mũi Họng\", \"value\": \"Niêm mạc họng đỏ xung huyết nhẹ, có nhiều hạt lympho nhỏ thành sau họng.\"}]",
            AttachmentsJson = "[{\"name\": \"Phieu_Noi_Soi_TMH.jpg\", \"data\": \"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==\"}]",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        _context.MedicalRecords.Add(record2);
        await _context.SaveChangesAsync();

        // Kê đơn cho bệnh án 2
        var pres2 = new Prescription
        {
            MedicalRecordId = record2.Id,
            Instructions = "Uống đều đặn mỗi ngày sau ăn.",
            PrescribedAt = DateTime.UtcNow.AddDays(-2),
            Details = new List<PrescriptionDetail>
            {
                new PrescriptionDetail
                {
                    MedicationId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    MedicationName = "Amoxicillin 500mg",
                    Quantity = 14,
                    Dosage = "Uống ngày 2 lần, mỗi lần 1 viên"
                }
            }
        };
        _context.Prescriptions.Add(pres2);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Đã đổ dữ liệu mẫu thành công! Bệnh nhân Nguyễn Văn Bệnh Nhân Demo (Id 3) hiện đã có 2 bệnh án đầy đủ chỉ số mới." });
    }

    // API: Lấy toàn bộ bệnh án hệ thống (Cho Admin)
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetAllRecords()
    {
        var records = await _context.MedicalRecords
            .Include(m => m.Prescription)
            .ThenInclude(p => p!.Details)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MedicalRecordResponseDto
            {
                Id = m.Id,
                PatientId = m.PatientId,
                DoctorId = m.DoctorId,
                Title = m.Title,
                Symptoms = m.Symptoms,
                Diagnosis = m.Diagnosis,
                Notes = m.Notes,
                Weight = m.Weight,
                Height = m.Height,
                BloodPressure = m.BloodPressure,
                HeartRate = m.HeartRate,
                Temperature = m.Temperature,
                CustomMetricsJson = m.CustomMetricsJson,
                AttachmentsJson = m.AttachmentsJson,
                CreatedAt = m.CreatedAt,
                Prescription = m.Prescription != null ? new PrescriptionResponseDto
                {
                    Id = m.Prescription.Id,
                    Instructions = m.Prescription.Instructions,
                    PrescribedAt = m.Prescription.PrescribedAt,
                    Details = m.Prescription.Details.Select(d => new PrescriptionDetailResponseDto
                    {
                        Id = d.Id,
                        MedicationId = d.MedicationId,
                        MedicationName = d.MedicationName,
                        Quantity = d.Quantity,
                        Dosage = d.Dosage
                    }).ToList()
                } : null
            }).ToListAsync();

        return Ok(records);
    }

    // API: Lấy toàn bộ lịch sử khám của 1 bệnh nhân
    [HttpGet("patient/{patientId}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetRecordsByPatient(Guid patientId)
    {
        var records = await _context.MedicalRecords
            .Include(m => m.Prescription)
            .ThenInclude(p => p!.Details)
            .Where(m => m.PatientId == patientId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MedicalRecordResponseDto
            {
                Id = m.Id,
                PatientId = m.PatientId,
                DoctorId = m.DoctorId,
                Title = m.Title,
                Symptoms = m.Symptoms,
                Diagnosis = m.Diagnosis,
                Notes = m.Notes,
                Weight = m.Weight,
                Height = m.Height,
                BloodPressure = m.BloodPressure,
                HeartRate = m.HeartRate,
                Temperature = m.Temperature,
                CustomMetricsJson = m.CustomMetricsJson,
                AttachmentsJson = m.AttachmentsJson,
                CreatedAt = m.CreatedAt,
                Prescription = m.Prescription != null ? new PrescriptionResponseDto
                {
                    Id = m.Prescription.Id,
                    Instructions = m.Prescription.Instructions,
                    PrescribedAt = m.Prescription.PrescribedAt,
                    Details = m.Prescription.Details.Select(d => new PrescriptionDetailResponseDto
                    {
                        Id = d.Id,
                        MedicationId = d.MedicationId,
                        MedicationName = d.MedicationName,
                        Quantity = d.Quantity,
                        Dosage = d.Dosage
                    }).ToList()
                } : null
            }).ToListAsync();

        return Ok(records);
    }

    // API: Lấy toàn bộ bệnh án của 1 bác sĩ
    [HttpGet("doctor/{doctorId}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetRecordsByDoctor(Guid doctorId)
    {
        var records = await _context.MedicalRecords
            .Include(m => m.Prescription)
            .ThenInclude(p => p!.Details)
            .Where(m => m.DoctorId == doctorId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MedicalRecordResponseDto
            {
                Id = m.Id,
                PatientId = m.PatientId,
                DoctorId = m.DoctorId,
                Title = m.Title,
                Symptoms = m.Symptoms,
                Diagnosis = m.Diagnosis,
                Notes = m.Notes,
                Weight = m.Weight,
                Height = m.Height,
                BloodPressure = m.BloodPressure,
                HeartRate = m.HeartRate,
                Temperature = m.Temperature,
                CustomMetricsJson = m.CustomMetricsJson,
                AttachmentsJson = m.AttachmentsJson,
                CreatedAt = m.CreatedAt,
                Prescription = m.Prescription != null ? new PrescriptionResponseDto
                {
                    Id = m.Prescription.Id,
                    Instructions = m.Prescription.Instructions,
                    PrescribedAt = m.Prescription.PrescribedAt,
                    Details = m.Prescription.Details.Select(d => new PrescriptionDetailResponseDto
                    {
                        Id = d.Id,
                        MedicationId = d.MedicationId,
                        MedicationName = d.MedicationName,
                        Quantity = d.Quantity,
                        Dosage = d.Dosage
                    }).ToList()
                } : null
            }).ToListAsync();

        return Ok(records);
    }

    // API: Bác sĩ ghi bệnh án mới
    [HttpPost]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> CreateRecord([FromBody] CreateMedicalRecordDto dto)
    {
        // Phải check xem thằng bệnh nhân này có tồn tại trong DB không đã
        var patientExists = await _context.Patients.AnyAsync(p => p.Id == dto.PatientId);
        if (!patientExists) 
        {
            // Lấy tên bệnh nhân từ DTO nếu có, không thì dùng mặc định
            var newPatient = new Patient
            {
                Id = dto.PatientId,
                GatewayPatientId = dto.GatewayPatientId,
                FullName = dto.PatientName ?? "Bệnh nhân Medicare",
                DateOfBirth = DateTime.Today.AddYears(-30),
                Gender = "Nam",
                MedicalHistory = "Khám lần đầu",
                Allergies = "Không có"
            };
            _context.Patients.Add(newPatient);
            await _context.SaveChangesAsync();
            Console.WriteLine($"🛡️ [MedicalRecordService Self-Heal] Tự động tạo hồ sơ Bệnh nhân (Id: {dto.PatientId}, GatewayId: {dto.GatewayPatientId}, Name: {newPatient.FullName})");
        }
        else
        {
            // Cập nhật GatewayPatientId nếu có (cho patient đã tồn tại)
            if (dto.GatewayPatientId.HasValue)
            {
                var patient = await _context.Patients.FindAsync(dto.PatientId);
                if (patient != null && patient.GatewayPatientId == null)
                {
                    patient.GatewayPatientId = dto.GatewayPatientId;
                    await _context.SaveChangesAsync();
                }
            }
        }

        var newRecord = new MedicalRecord
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            Title = dto.Title,
            Symptoms = dto.Symptoms,
            Diagnosis = dto.Diagnosis,
            Notes = dto.Notes,
            Weight = dto.Weight,
            Height = dto.Height,
            BloodPressure = dto.BloodPressure,
            HeartRate = dto.HeartRate,
            Temperature = dto.Temperature,
            CustomMetricsJson = dto.CustomMetricsJson,
            AttachmentsJson = dto.AttachmentsJson
        };

        _context.MedicalRecords.Add(newRecord);
        await _context.SaveChangesAsync();

        int targetUserId = dto.GatewayPatientId ?? 0;
        if (targetUserId == 0)
        {
            var p = await _context.Patients.FindAsync(dto.PatientId);
            if (p != null && p.GatewayPatientId.HasValue)
            {
                targetUserId = p.GatewayPatientId.Value;
            }
            else
            {
                // Fallback: try parsing from GUID last segment
                var guidString = dto.PatientId.ToString();
                var lastSegment = guidString.Split('-').Last().TrimStart('0');
                if (!string.IsNullOrEmpty(lastSegment) && int.TryParse(lastSegment, out int parsedInt))
                {
                    targetUserId = parsedInt;
                }
            }
        }

        // [CROSS-SERVICE SYNC] Cập nhật trạng thái Appointment thành "Đã khám" (Status = 2)
        if (dto.AppointmentId.HasValue)
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            _ = Task.Run(async () =>
            {
                try
                {
                    var appointmentUrl = _configuration["APPOINTMENT_SERVICE_URL"] ?? "http://localhost:5150";
                    var client = _httpClientFactory.CreateClient();
                    // Forward JWT token cho Appointment-Service xác thực
                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        client.DefaultRequestHeaders.Add("Authorization", authHeader);
                    }
                    var response = await client.PutAsync($"{appointmentUrl.TrimEnd('/')}/api/Appointments/{dto.AppointmentId}/complete", null);
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✅ [Cross-Service] Đã cập nhật Appointment {dto.AppointmentId} → Đã khám xong");
                        // Gửi thông báo cho bệnh nhân
                        var notifClient = _httpClientFactory.CreateClient();
                        notifClient.DefaultRequestHeaders.Add("X-Service-API-Key", "MedicareServiceInternalKey2024");
                        var notifPayload = new StringContent(
                            System.Text.Json.JsonSerializer.Serialize(new {
                                userId = targetUserId,
                                title = "Khám bệnh hoàn tất",
                                message = $"Bác sĩ đã hoàn tất khám bệnh. Chẩn đoán: {dto.Diagnosis}",
                                type = "success"
                            }),
                            Encoding.UTF8, "application/json"
                        );
                        var notifRes = await notifClient.PostAsync($"{appointmentUrl.TrimEnd('/')}/api/Notifications/create-direct", notifPayload);
                        if (notifRes.IsSuccessStatusCode)
                            Console.WriteLine($"✅ [Cross-Service] Đã gửi thông báo khám xong cho Appointment {dto.AppointmentId} đến Patient ID {targetUserId}");
                    }
                    else
                        Console.WriteLine($"⚠️ [Cross-Service] Không thể cập nhật Appointment: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ [Cross-Service] Lỗi callback Appointment: {ex.Message}");
                }
            });
        }

        return Ok(new { 
            Message = "Ghi bệnh án thành công!", 
            RecordId = newRecord.Id 
        });
    }

    // API: Cập nhật bệnh án
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> UpdateRecord(Guid id, [FromBody] CreateMedicalRecordDto dto)
    {
        var record = await _context.MedicalRecords.FindAsync(id);
        if (record == null)
        {
            return NotFound("Không tìm thấy bệnh án cần cập nhật.");
        }

        record.Title = dto.Title;
        record.Symptoms = dto.Symptoms;
        record.Diagnosis = dto.Diagnosis;
        record.Notes = dto.Notes;
        record.Weight = dto.Weight;
        record.Height = dto.Height;
        record.BloodPressure = dto.BloodPressure;
        record.HeartRate = dto.HeartRate;
        record.Temperature = dto.Temperature;
        record.CustomMetricsJson = dto.CustomMetricsJson;
        record.AttachmentsJson = dto.AttachmentsJson;

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Cập nhật bệnh án thành công!" });
    }

    // API: Xóa 1 bệnh án (kèm đơn thuốc nếu có)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> DeleteRecord(Guid id)
    {
        var record = await _context.MedicalRecords
            .Include(m => m.Prescription)
            .ThenInclude(p => p!.Details)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (record == null)
            return NotFound("Không tìm thấy bệnh án cần xóa.");

        _context.MedicalRecords.Remove(record);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Đã xóa bệnh án thành công!" });
    }
}