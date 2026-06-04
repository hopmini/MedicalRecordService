using MedicalRecordService.Data;
using MedicalRecordService.DTOs;
using MedicalRecordService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalRecordService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicalRecordsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public MedicalRecordsController(MedicalDbContext context)
    {
        _context = context;
    }

    // API: Lấy toàn bộ bệnh án hệ thống (Cho Admin)
    [HttpGet]
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
    public async Task<IActionResult> GetRecordsByPatient(Guid patientId)
    {
        var records = await _context.MedicalRecords
            .Include(m => m.Prescription)
            .ThenInclude(p => p!.Details)
            .Where(m => m.PatientId == patientId)
            .OrderByDescending(m => m.CreatedAt) // Sắp xếp mới nhất lên đầu
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
    public async Task<IActionResult> CreateRecord([FromBody] CreateMedicalRecordDto dto)
    {
        // Phải check xem thằng bệnh nhân này có tồn tại trong DB không đã
        var patientExists = await _context.Patients.AnyAsync(p => p.Id == dto.PatientId);
        if (!patientExists) 
        {
            var newPatient = new Patient
            {
                Id = dto.PatientId,
                FullName = "Bệnh nhân Medicare",
                DateOfBirth = DateTime.Today.AddYears(-30),
                Gender = "Nam",
                MedicalHistory = "Khám lần đầu",
                Allergies = "Không có"
            };
            _context.Patients.Add(newPatient);
            await _context.SaveChangesAsync();
            Console.WriteLine($"🛡️ [MedicalRecordService Self-Heal] Tự động tạo hồ sơ Bệnh nhân (Id: {dto.PatientId})");
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

        return Ok(new { 
            Message = "Ghi bệnh án thành công!", 
            RecordId = newRecord.Id 
        });
    }
}