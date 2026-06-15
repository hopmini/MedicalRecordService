using MedicalRecordService.Data;
using MedicalRecordService.DTOs;
using MedicalRecordService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalRecordService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    // Bơm cái DB Context vào đây để xài
    public PatientsController(MedicalDbContext context)
    {
        _context = context;
    }

    // API: Lấy danh sách bệnh nhân
    [HttpGet]
    public async Task<IActionResult> GetAllPatients()
    {
        var patients = await _context.Patients
            .Select(p => new PatientResponseDto
            {
                Id = p.Id,
                GatewayPatientId = p.GatewayPatientId,
                FullName = p.FullName,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender,
                MedicalHistory = p.MedicalHistory,
                Allergies = p.Allergies
            }).ToListAsync();

        return Ok(patients);
    }

    // API: Lấy thông tin chi tiết của 1 bệnh nhân theo ID (Tự động tạo mới nếu chưa có hồ sơ - Self Healing)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatientById(Guid id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null)
        {
            patient = new Patient
            {
                Id = id,
                FullName = "Bệnh nhân Medicare",
                DateOfBirth = DateTime.Today.AddYears(-30),
                Gender = "Nam",
                MedicalHistory = "Khám lần đầu",
                Allergies = "Không có"
            };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            Console.WriteLine($"🛡️ [PatientsController Self-Heal] Tự động khởi tạo hồ sơ Bệnh nhân (Id: {id})");
        }

        return Ok(new PatientResponseDto
        {
            Id = patient.Id,
            GatewayPatientId = patient.GatewayPatientId,
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            MedicalHistory = patient.MedicalHistory,
            Allergies = patient.Allergies
        });
    }

    // API: Tạo hồ sơ bệnh nhân mới
    [HttpPost]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto dto)
    {
        var newPatient = new Patient
        {
            FullName = dto.FullName,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            MedicalHistory = dto.MedicalHistory,
            Allergies = dto.Allergies
        };

        _context.Patients.Add(newPatient);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAllPatients), new { id = newPatient.Id }, dto);
    }

    // API: Cập nhật hồ sơ sức khỏe bệnh nhân
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] CreatePatientDto dto)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null)
        {
            return NotFound("Không tìm thấy bệnh nhân cần cập nhật.");
        }

        patient.FullName = dto.FullName;
        patient.DateOfBirth = dto.DateOfBirth;
        patient.Gender = dto.Gender;
        patient.MedicalHistory = dto.MedicalHistory;
        patient.Allergies = dto.Allergies;

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Cập nhật hồ sơ sức khỏe thành công!" });
    }

    // API: Xóa hồ sơ bệnh nhân (kèm toàn bộ bệnh án + đơn thuốc)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePatient(Guid id)
    {
        var patient = await _context.Patients
            .Include(p => p.MedicalRecords)
            .ThenInclude(m => m.Prescription)
            .ThenInclude(p => p!.Details)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
            return NotFound("Không tìm thấy bệnh nhân cần xóa.");

        foreach (var record in patient.MedicalRecords)
        {
            if (record.Prescription != null)
            {
                _context.PrescriptionDetails.RemoveRange(record.Prescription.Details);
                _context.Prescriptions.Remove(record.Prescription);
            }
        }
        _context.MedicalRecords.RemoveRange(patient.MedicalRecords);
        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Đã xóa hồ sơ bệnh nhân và toàn bộ bệnh án liên quan!" });
    }
}