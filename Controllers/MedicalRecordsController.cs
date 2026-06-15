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
                Symptoms = m.Symptoms,
                Diagnosis = m.Diagnosis,
                Notes = m.Notes,
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
                Symptoms = m.Symptoms,
                Diagnosis = m.Diagnosis,
                Notes = m.Notes,
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
                Symptoms = m.Symptoms,
                Diagnosis = m.Diagnosis,
                Notes = m.Notes,
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
            Symptoms = dto.Symptoms,
            Diagnosis = dto.Diagnosis,
            Notes = dto.Notes
        };

        _context.MedicalRecords.Add(newRecord);
        await _context.SaveChangesAsync();

        // [CROSS-SERVICE SYNC] Cập nhật trạng thái Appointment thành "Đã khám" (Status = 2)
        if (dto.AppointmentId.HasValue)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var appointmentUrl = _configuration["APPOINTMENT_SERVICE_URL"] ?? "http://localhost:5150";
                    var client = _httpClientFactory.CreateClient();
                    // Forward JWT token cho Appointment-Service xác thực
                    var authHeader = Request.Headers["Authorization"].FirstOrDefault();
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
                                userId = 0,
                                title = "Khám bệnh hoàn tất",
                                message = $"Bác sĩ đã hoàn tất khám bệnh. Chẩn đoán: {dto.Diagnosis}",
                                type = "success"
                            }),
                            Encoding.UTF8, "application/json"
                        );
                        var notifRes = await notifClient.PostAsync($"{appointmentUrl.TrimEnd('/')}/api/Notifications/create-direct", notifPayload);
                        if (notifRes.IsSuccessStatusCode)
                            Console.WriteLine($"✅ [Cross-Service] Đã gửi thông báo khám xong cho Appointment {dto.AppointmentId}");
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