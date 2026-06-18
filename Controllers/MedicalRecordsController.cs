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

    private static MedicalRecordResponseDto MapRecord(MedicalRecord m)
    {
        return new MedicalRecordResponseDto
        {
            Id = m.Id,
            PatientId = m.PatientId,
            GatewayPatientId = m.Patient.GatewayPatientId,
            DoctorId = m.DoctorId,
            Title = m.Title,
            Symptoms = m.Symptoms,
            Diagnosis = m.Diagnosis,
            PreliminaryDiagnosis = m.PreliminaryDiagnosis,
            FinalDiagnosis = m.FinalDiagnosis,
            MedicalHistorySnapshot = m.MedicalHistorySnapshot,
            AllergiesSnapshot = m.AllergiesSnapshot,
            DiagnosisCode = m.DiagnosisCode,
            DiagnosisCodeName = m.DiagnosisCodeName,
            AdmissionDate = m.AdmissionDate,
            DischargeDate = m.DischargeDate,
            DischargeDiagnosis = m.DischargeDiagnosis,
            DischargeInstructions = m.DischargeInstructions,
            FollowUpInstructions = m.FollowUpInstructions,
            FollowUpClinic = m.FollowUpClinic,
            FollowUpDate = m.FollowUpDate,
            Notes = m.Notes,
            Weight = m.Weight,
            Height = m.Height,
            BloodPressure = m.BloodPressure,
            HeartRate = m.HeartRate,
            Temperature = m.Temperature,
            CustomMetricsJson = m.CustomMetricsJson,
            AttachmentsJson = m.AttachmentsJson,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt,
            UpdatedBy = m.UpdatedBy,
            Status = m.Status.ToString(),
            IsDeleted = m.IsDeleted,
            IsLocked = m.IsLocked,
            LockedBy = m.LockedBy,
            Prescription = m.Prescription != null ? new PrescriptionResponseDto
            {
                Id = m.Prescription.Id,
                Instructions = m.Prescription.Instructions,
                PrescribedAt = m.Prescription.PrescribedAt,
                Status = m.Prescription.Status,
                ExpiryDate = m.Prescription.ExpiryDate,
                RefillCount = m.Prescription.RefillCount,
                Details = m.Prescription.Details.Select(d => new PrescriptionDetailResponseDto
                {
                    Id = d.Id,
                    MedicationId = d.MedicationId,
                    MedicationName = d.MedicationName,
                    Quantity = d.Quantity,
                    Dosage = d.Dosage
                }).ToList()
            } : null,
            LabTests = m.LabTests?.Select(lt => new LabTestResponseDto
            {
                Id = lt.Id,
                MedicalRecordId = lt.MedicalRecordId,
                TestName = lt.TestName,
                TestCode = lt.TestCode,
                Result = lt.Result,
                NormalRange = lt.NormalRange,
                Unit = lt.Unit,
                Status = lt.Status.ToString(),
                Notes = lt.Notes,
                AttachmentFileJson = lt.AttachmentFileJson,
                PerformedBy = lt.PerformedBy,
                RequestedAt = lt.RequestedAt,
                CompletedAt = lt.CompletedAt
            }).ToList(),
            TreatmentPlans = m.TreatmentPlans?.Select(tp => new TreatmentPlanResponseDto
            {
                Id = tp.Id,
                MedicalRecordId = tp.MedicalRecordId,
                PlanName = tp.PlanName,
                Description = tp.Description,
                StartDate = tp.StartDate,
                EndDate = tp.EndDate,
                Status = tp.Status.ToString(),
                CreatedBy = tp.CreatedBy,
                CreatedAt = tp.CreatedAt,
                Progressions = tp.Progressions?.Select(p => new TreatmentProgressionResponseDto
                {
                    Id = p.Id,
                    RecordedAt = p.RecordedAt,
                    Notes = p.Notes,
                    Status = p.Status,
                    RecordedBy = p.RecordedBy
                }).ToList()
            }).ToList()
        };
    }

    [HttpGet("seed-demo")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedDemoData()
    {
        var patientId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        var doctorId = Guid.Parse("b7201c65-934a-461c-8f0f-7bd158215e43");

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

        var record1 = new MedicalRecord
        {
            PatientId = patientId,
            DoctorId = doctorId,
            Title = "Khám Tổng Quát & Tầm Soát Tim Mạch",
            Symptoms = "Đau ngực nhẹ khi vận động mạnh, thỉnh thoảng khó thở về đêm.",
            Diagnosis = "Trào ngược dạ dày thực quản (GERD) mức độ nhẹ, theo dõi rối loạn nhịp tim chậm.",
            DiagnosisCode = "K21",
            DiagnosisCodeName = "Trào ngược dạ dày-thực quản",
            Notes = "Tránh thức ăn cay nóng, đồ uống có gas. Không ăn muộn sau 20h.",
            Weight = 72.5, Height = 175.0, BloodPressure = "125/82", HeartRate = 68, Temperature = 36.6,
            CustomMetricsJson = "[{\"name\": \"ECG\", \"value\": \"Nhịp xoang đều 68 ck/ph\"}, {\"name\": \"Glucose máu\", \"value\": \"5.4 mmol/L\"}, {\"name\": \"Cholesterol TP\", \"value\": \"4.8 mmol/L\"}]",
            AttachmentsJson = "[{\"name\": \"Ket_Qua_Sieu_Am_Tim.pdf\", \"data\": \"data:application/pdf;base64,\"}]",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        _context.MedicalRecords.Add(record1);
        await _context.SaveChangesAsync();

        var pres1 = new Prescription
        {
            MedicalRecordId = record1.Id,
            Instructions = "Uống thuốc sau bữa ăn sáng và tối. Tái khám sau 2 tuần.",
            PrescribedAt = DateTime.UtcNow.AddDays(-10),
            Details = new List<PrescriptionDetail>
            {
                new() { MedicationId = Guid.Parse("00000000-0000-0000-0000-000000000001"), MedicationName = "Panadol Extra 500mg", Quantity = 20, Dosage = "Sáng 1 viên, Tối 1 viên" }
            }
        };
        _context.Prescriptions.Add(pres1);
        await _context.SaveChangesAsync();

        var record2 = new MedicalRecord
        {
            PatientId = patientId, DoctorId = doctorId,
            Title = "Khám Chuyên Khoa Tai Mũi Họng",
            Symptoms = "Ho khan kéo dài, đau rát họng, có đờm nhẹ.",
            Diagnosis = "Viêm họng hạt cấp tính, trào ngược thực quản.",
            DiagnosisCode = "J02",
            DiagnosisCodeName = "Viêm họng cấp",
            Notes = "Súc họng nước muối ấm 3 lần/ngày. Giữ ấm vùng cổ.",
            Weight = 73.0, Height = 175.0, BloodPressure = "120/80", HeartRate = 72, Temperature = 37.2,
            AttachmentsJson = "[{\"name\": \"Phieu_Noi_Soi_TMH.jpg\", \"data\": \"data:image/png;base64,\"}]",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };
        _context.MedicalRecords.Add(record2);
        await _context.SaveChangesAsync();

        var pres2 = new Prescription
        {
            MedicalRecordId = record2.Id,
            Instructions = "Uống đều đặn mỗi ngày sau ăn.",
            PrescribedAt = DateTime.UtcNow.AddDays(-2),
            Details = new List<PrescriptionDetail>
            {
                new() { MedicationId = Guid.Parse("00000000-0000-0000-0000-000000000002"), MedicationName = "Amoxicillin 500mg", Quantity = 14, Dosage = "Uống ngày 2 lần, mỗi lần 1 viên" }
            }
        };
        _context.Prescriptions.Add(pres2);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Đã đổ dữ liệu mẫu thành công!" });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    public async Task<IActionResult> GetAllRecords(
        [FromQuery] string? q,
        [FromQuery] string? diagnosisCode,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? doctorId,
        [FromQuery] string? patientCode,
        [FromQuery] string? recordCode,
        [FromQuery] string? status,
        [FromQuery] string? patientName,
        [FromQuery] bool? includeDeleted)
    {
        var query = _context.MedicalRecords
            .Include(m => m.Prescription)
            .ThenInclude(p => p!.Details)
            .Include(m => m.LabTests)
            .Include(m => m.TreatmentPlans)
            .ThenInclude(tp => tp.Progressions)
            .AsQueryable();

        if (includeDeleted != true)
            query = query.Where(m => !m.IsDeleted);

        if (!string.IsNullOrEmpty(q))
        {
            var lower = q.ToLower();
            query = query.Where(m => m.Symptoms.ToLower().Contains(lower)
                || m.Diagnosis.ToLower().Contains(lower)
                || (m.DiagnosisCode != null && m.DiagnosisCode.ToLower().Contains(lower))
                || (m.Notes != null && m.Notes.ToLower().Contains(lower))
                || (m.Title != null && m.Title.ToLower().Contains(lower)));
        }
        if (!string.IsNullOrEmpty(diagnosisCode))
            query = query.Where(m => m.DiagnosisCode != null && m.DiagnosisCode == diagnosisCode);
        if (dateFrom.HasValue)
            query = query.Where(m => m.CreatedAt >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(m => m.CreatedAt <= dateTo.Value);
        if (!string.IsNullOrEmpty(doctorId) && Guid.TryParse(doctorId, out var docGuid))
            query = query.Where(m => m.DoctorId == docGuid);
        if (!string.IsNullOrEmpty(recordCode) && Guid.TryParse(recordCode, out var recGuid))
            query = query.Where(m => m.Id == recGuid);
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<RecordStatus>(status, true, out var recStatus))
            query = query.Where(m => m.Status == recStatus);
        if (!string.IsNullOrEmpty(patientName))
        {
            var lower = patientName.ToLower();
            query = query.Where(m => m.Patient.FullName.ToLower().Contains(lower));
        }

        var records = await query
            .Include(m => m.Patient)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => MapRecord(m))
            .ToListAsync();

        return Ok(records);
    }

    [HttpGet("patient/{patientId}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetRecordsByPatient(Guid patientId)
    {
        var records = await _context.MedicalRecords
            .Include(m => m.Patient)
            .Include(m => m.Prescription).ThenInclude(p => p!.Details)
            .Where(m => m.PatientId == patientId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => MapRecord(m))
            .ToListAsync();
        return Ok(records);
    }

    [HttpGet("doctor/{doctorId}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetRecordsByDoctor(Guid doctorId)
    {
        var records = await _context.MedicalRecords
            .Include(m => m.Patient)
            .Include(m => m.Prescription).ThenInclude(p => p!.Details)
            .Where(m => m.DoctorId == doctorId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => MapRecord(m))
            .ToListAsync();
        return Ok(records);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetRecord(Guid id)
    {
        var record = await _context.MedicalRecords
            .Include(m => m.Patient)
            .Include(m => m.Prescription).ThenInclude(p => p!.Details)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (record == null) return NotFound();
        return Ok(MapRecord(record));
    }

    [HttpPost]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> CreateRecord([FromBody] CreateMedicalRecordDto dto)
    {
        var patientExists = await _context.Patients.AnyAsync(p => p.Id == dto.PatientId);
        if (!patientExists)
        {
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
        }
        else if (dto.GatewayPatientId.HasValue)
        {
            var patient = await _context.Patients.FindAsync(dto.PatientId);
            if (patient != null && patient.GatewayPatientId == null)
            {
                patient.GatewayPatientId = dto.GatewayPatientId;
                await _context.SaveChangesAsync();
            }
        }

        var userName = User.Identity?.Name ?? "Unknown";

        var newRecord = new MedicalRecord
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            Title = dto.Title,
            Symptoms = dto.Symptoms,
            Diagnosis = dto.Diagnosis,
            DiagnosisCode = dto.DiagnosisCode,
            DiagnosisCodeName = dto.DiagnosisCodeName,
            AdmissionDate = dto.AdmissionDate,
            DischargeDate = dto.DischargeDate,
            DischargeDiagnosis = dto.DischargeDiagnosis,
            DischargeInstructions = dto.DischargeInstructions,
            FollowUpInstructions = dto.FollowUpInstructions,
            FollowUpClinic = dto.FollowUpClinic,
            FollowUpDate = dto.FollowUpDate,
            Notes = dto.Notes,
            Weight = dto.Weight, Height = dto.Height,
            BloodPressure = dto.BloodPressure, HeartRate = dto.HeartRate, Temperature = dto.Temperature,
            CustomMetricsJson = dto.CustomMetricsJson,
            AttachmentsJson = dto.AttachmentsJson,
            UpdatedBy = userName
        };

        _context.MedicalRecords.Add(newRecord);
        await _context.SaveChangesAsync();

        int targetUserId = dto.GatewayPatientId ?? 0;
        if (targetUserId == 0)
        {
            var p = await _context.Patients.FindAsync(dto.PatientId);
            if (p?.GatewayPatientId != null) targetUserId = p.GatewayPatientId.Value;
            else
            {
                var lastSegment = dto.PatientId.ToString().Split('-').Last().TrimStart('0');
                if (!string.IsNullOrEmpty(lastSegment) && int.TryParse(lastSegment, out var parsed))
                    targetUserId = parsed;
            }
        }

        if (dto.AppointmentId.HasValue)
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            _ = Task.Run(async () =>
            {
                try
                {
                    var appointmentUrl = _configuration["APPOINTMENT_SERVICE_URL"] ?? "http://localhost:5150";
                    var client = _httpClientFactory.CreateClient();
                    if (!string.IsNullOrEmpty(authHeader))
                        client.DefaultRequestHeaders.Add("Authorization", authHeader);
                    var response = await client.PutAsync($"{appointmentUrl.TrimEnd('/')}/api/Appointments/{dto.AppointmentId}/complete", null);
                    if (response.IsSuccessStatusCode)
                    {
                        var notifClient = _httpClientFactory.CreateClient();
                        notifClient.DefaultRequestHeaders.Add("X-Service-API-Key", "MedicareServiceInternalKey2024");
                        var notifPayload = new StringContent(
                            System.Text.Json.JsonSerializer.Serialize(new { userId = targetUserId, title = "Khám bệnh hoàn tất", message = $"Chẩn đoán: {dto.Diagnosis}", type = "success" }),
                            Encoding.UTF8, "application/json");
                        await notifClient.PostAsync($"{appointmentUrl.TrimEnd('/')}/api/Notifications/create-direct", notifPayload);
                    }
                }
                catch (Exception ex) { Console.WriteLine($"⚠️ [Cross-Service] Lỗi callback: {ex.Message}"); }
            });
        }

        return Ok(new { Message = "Ghi bệnh án thành công!", RecordId = newRecord.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> UpdateRecord(Guid id, [FromBody] UpdateMedicalRecordDto dto)
    {
        var record = await _context.MedicalRecords.FindAsync(id);
        if (record == null) return NotFound("Không tìm thấy bệnh án.");

        var userName = User.Identity?.Name ?? "Unknown";

        if (dto.Title != null) record.Title = dto.Title;
        if (dto.Symptoms != null) record.Symptoms = dto.Symptoms;
        if (dto.Diagnosis != null) record.Diagnosis = dto.Diagnosis;
        if (dto.DiagnosisCode != null) record.DiagnosisCode = dto.DiagnosisCode;
        if (dto.DiagnosisCodeName != null) record.DiagnosisCodeName = dto.DiagnosisCodeName;
        if (dto.AdmissionDate != null) record.AdmissionDate = dto.AdmissionDate;
        if (dto.DischargeDate != null) record.DischargeDate = dto.DischargeDate;
        if (dto.DischargeDiagnosis != null) record.DischargeDiagnosis = dto.DischargeDiagnosis;
        if (dto.DischargeInstructions != null) record.DischargeInstructions = dto.DischargeInstructions;
        if (dto.FollowUpInstructions != null) record.FollowUpInstructions = dto.FollowUpInstructions;
        if (dto.FollowUpClinic != null) record.FollowUpClinic = dto.FollowUpClinic;
        if (dto.FollowUpDate != null) record.FollowUpDate = dto.FollowUpDate;
        if (dto.Notes != null) record.Notes = dto.Notes;
        if (dto.Weight != null) record.Weight = dto.Weight;
        if (dto.Height != null) record.Height = dto.Height;
        if (dto.BloodPressure != null) record.BloodPressure = dto.BloodPressure;
        if (dto.HeartRate != null) record.HeartRate = dto.HeartRate;
        if (dto.Temperature != null) record.Temperature = dto.Temperature;
        if (dto.CustomMetricsJson != null) record.CustomMetricsJson = dto.CustomMetricsJson;
        if (dto.AttachmentsJson != null) record.AttachmentsJson = dto.AttachmentsJson;

        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = userName;
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Cập nhật bệnh án thành công!" });
    }

    [HttpPut("{id}/discharge")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> UpdateDischargeSummary(Guid id, [FromBody] UpdateMedicalRecordDto dto)
    {
        var record = await _context.MedicalRecords.FindAsync(id);
        if (record == null) return NotFound();

        record.DischargeDate = dto.DischargeDate ?? DateTime.UtcNow;
        if (dto.DischargeDiagnosis != null) record.DischargeDiagnosis = dto.DischargeDiagnosis;
        if (dto.DischargeInstructions != null) record.DischargeInstructions = dto.DischargeInstructions;
        if (dto.FollowUpInstructions != null) record.FollowUpInstructions = dto.FollowUpInstructions;
        if (dto.FollowUpClinic != null) record.FollowUpClinic = dto.FollowUpClinic;
        if (dto.FollowUpDate != null) record.FollowUpDate = dto.FollowUpDate;
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = User.Identity?.Name ?? "Unknown";
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Cập nhật giấy ra viện thành công!" });
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
    {
        var record = await _context.MedicalRecords.FindAsync(id);
        if (record == null) return NotFound("Không tìm thấy bệnh án.");
        if (record.IsLocked) return BadRequest("Bệnh án đã bị khóa, không thể thay đổi trạng thái.");

        if (!Enum.TryParse<RecordStatus>(dto.Status, true, out var newStatus))
            return BadRequest("Trạng thái không hợp lệ.");

        var oldStatus = record.Status.ToString();
        record.Status = newStatus;
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = User.Identity?.Name ?? "Unknown";

        _context.AuditLogEntries.Add(new AuditLogEntry
        {
            RecordId = id,
            Field = "Status",
            OldValue = oldStatus,
            NewValue = newStatus.ToString(),
            ChangedBy = User.Identity?.Name ?? "Unknown"
        });

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Cập nhật trạng thái thành công!", Status = newStatus.ToString() });
    }

    [HttpPut("{id}/lock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> LockRecord(Guid id)
    {
        var record = await _context.MedicalRecords.FindAsync(id);
        if (record == null) return NotFound("Không tìm thấy bệnh án.");
        if (record.IsLocked) return BadRequest("Bệnh án đã bị khóa.");

        record.IsLocked = true;
        record.LockedAt = DateTime.UtcNow;
        record.LockedBy = User.Identity?.Name ?? "Unknown";
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = record.LockedBy;

        _context.AuditLogEntries.Add(new AuditLogEntry
        {
            RecordId = id,
            Field = "IsLocked",
            OldValue = "false",
            NewValue = "true",
            ChangedBy = record.LockedBy
        });

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã khóa bệnh án." });
    }

    [HttpPut("{id}/unlock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnlockRecord(Guid id)
    {
        var record = await _context.MedicalRecords.FindAsync(id);
        if (record == null) return NotFound("Không tìm thấy bệnh án.");
        if (!record.IsLocked) return BadRequest("Bệnh án chưa bị khóa.");

        record.IsLocked = false;
        record.LockedAt = null;
        record.LockedBy = null;
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = User.Identity?.Name ?? "Unknown";

        _context.AuditLogEntries.Add(new AuditLogEntry
        {
            RecordId = id,
            Field = "IsLocked",
            OldValue = "true",
            NewValue = "false",
            ChangedBy = record.UpdatedBy
        });

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã mở khóa bệnh án." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> DeleteRecord(Guid id)
    {
        var record = await _context.MedicalRecords
            .Include(m => m.Prescription).ThenInclude(p => p!.Details)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (record == null) return NotFound();
        _context.MedicalRecords.Remove(record);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã xóa bệnh án thành công!" });
    }

    [HttpPut("{id}/soft-delete")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> SoftDeleteRecord(Guid id)
    {
        var record = await _context.MedicalRecords.FindAsync(id);
        if (record == null) return NotFound("Không tìm thấy bệnh án.");
        if (record.IsDeleted) return BadRequest("Bệnh án đã bị xóa.");

        record.IsDeleted = true;
        record.DeletedAt = DateTime.UtcNow;
        record.DeletedBy = User.Identity?.Name ?? "Unknown";
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = record.DeletedBy;

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã xóa bệnh án (soft delete)." });
    }

    [HttpPut("{id}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RestoreRecord(Guid id)
    {
        var record = await _context.MedicalRecords.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Id == id);
        if (record == null) return NotFound("Không tìm thấy bệnh án.");
        if (!record.IsDeleted) return BadRequest("Bệnh án chưa bị xóa.");

        record.IsDeleted = false;
        record.DeletedAt = null;
        record.DeletedBy = null;
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = User.Identity?.Name ?? "Unknown";

        _context.AuditLogEntries.Add(new AuditLogEntry
        {
            RecordId = id,
            Field = "IsDeleted",
            OldValue = "true",
            NewValue = "false",
            ChangedBy = record.UpdatedBy
        });

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã khôi phục bệnh án." });
    }

    [HttpGet("deleted")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDeletedRecords()
    {
        var records = await _context.MedicalRecords.IgnoreQueryFilters()
            .Include(m => m.Patient)
            .Where(m => m.IsDeleted)
            .OrderByDescending(m => m.DeletedAt)
            .Select(m => MapRecord(m))
            .ToListAsync();
        return Ok(records);
    }

    [HttpGet("{id}/history")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetRecordHistory(Guid id)
    {
        var entries = await _context.AuditLogEntries
            .Where(a => a.RecordId == id)
            .OrderByDescending(a => a.ChangedAt)
            .Select(a => new AuditLogEntryResponseDto
            {
                Id = a.Id,
                RecordId = a.RecordId,
                Field = a.Field,
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                ChangedBy = a.ChangedBy,
                ChangedAt = a.ChangedAt
            })
            .ToListAsync();
        return Ok(entries);
    }

    [HttpPost("{id}/lab-tests")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> AddLabTest(Guid id, [FromBody] CreateLabTestDto dto)
    {
        var record = await _context.MedicalRecords.FindAsync(id);
        if (record == null) return NotFound("Không tìm thấy bệnh án.");
        if (record.IsLocked) return BadRequest("Bệnh án đã bị khóa.");

        var labTest = new LabTest
        {
            MedicalRecordId = id,
            TestName = dto.TestName,
            TestCode = dto.TestCode,
            NormalRange = dto.NormalRange,
            Unit = dto.Unit,
            Notes = dto.Notes
        };
        _context.LabTests.Add(labTest);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã thêm yêu cầu xét nghiệm.", LabTestId = labTest.Id });
    }

    [HttpGet("{id}/lab-tests")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetLabTests(Guid id)
    {
        var tests = await _context.LabTests
            .Where(l => l.MedicalRecordId == id)
            .OrderByDescending(l => l.RequestedAt)
            .Select(l => new LabTestResponseDto
            {
                Id = l.Id,
                MedicalRecordId = l.MedicalRecordId,
                TestName = l.TestName,
                TestCode = l.TestCode,
                Result = l.Result,
                NormalRange = l.NormalRange,
                Unit = l.Unit,
                Status = l.Status.ToString(),
                Notes = l.Notes,
                AttachmentFileJson = l.AttachmentFileJson,
                PerformedBy = l.PerformedBy,
                RequestedAt = l.RequestedAt,
                CompletedAt = l.CompletedAt
            })
            .ToListAsync();
        return Ok(tests);
    }

    [HttpPost("{id}/treatment-plans")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> AddTreatmentPlan(Guid id, [FromBody] CreateTreatmentPlanDto dto)
    {
        var record = await _context.MedicalRecords.FindAsync(id);
        if (record == null) return NotFound("Không tìm thấy bệnh án.");
        if (record.IsLocked) return BadRequest("Bệnh án đã bị khóa.");

        var plan = new TreatmentPlan
        {
            MedicalRecordId = id,
            PlanName = dto.PlanName,
            Description = dto.Description,
            StartDate = dto.StartDate ?? DateTime.UtcNow,
            EndDate = dto.EndDate,
            CreatedBy = User.Identity?.Name ?? "Unknown"
        };
        _context.TreatmentPlans.Add(plan);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã thêm phác đồ điều trị.", PlanId = plan.Id });
    }

    [HttpGet("{id}/treatment-plans")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetTreatmentPlans(Guid id)
    {
        var plans = await _context.TreatmentPlans
            .Include(t => t.Progressions)
            .Where(t => t.MedicalRecordId == id)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TreatmentPlanResponseDto
            {
                Id = t.Id,
                MedicalRecordId = t.MedicalRecordId,
                PlanName = t.PlanName,
                Description = t.Description,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Status = t.Status.ToString(),
                CreatedBy = t.CreatedBy,
                CreatedAt = t.CreatedAt,
                Progressions = t.Progressions.Select(p => new TreatmentProgressionResponseDto
                {
                    Id = p.Id,
                    RecordedAt = p.RecordedAt,
                    Notes = p.Notes,
                    Status = p.Status,
                    RecordedBy = p.RecordedBy
                }).ToList()
            })
            .ToListAsync();
        return Ok(plans);
    }
}
