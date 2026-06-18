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
public class PrescriptionsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public PrescriptionsController(MedicalDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pres = await _context.Prescriptions
            .Include(p => p.Details)
            .OrderByDescending(p => p.PrescribedAt)
            .Select(p => new
            {
                p.Id,
                p.MedicalRecordId,
                p.Instructions,
                p.PrescribedAt,
                p.Status,
                p.ExpiryDate,
                p.RefillCount,
                Details = p.Details.Select(d => new
                {
                    d.Id,
                    d.MedicationId,
                    d.MedicationName,
                    d.Quantity,
                    d.Dosage
                }).ToList()
            })
            .ToListAsync();
        return Ok(pres);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var p = await _context.Prescriptions
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (p == null) return NotFound();
        return Ok(new
        {
            p.Id, p.MedicalRecordId, p.Instructions, p.PrescribedAt,
            p.Status, p.ExpiryDate, p.RefillCount,
            Details = p.Details.Select(d => new { d.Id, d.MedicationId, d.MedicationName, d.Quantity, d.Dosage }).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreatePrescription([FromBody] CreatePrescriptionDto dto)
    {
        var record = await _context.MedicalRecords
            .Include(m => m.Prescription)
            .Include(m => m.Patient)
            .FirstOrDefaultAsync(m => m.Id == dto.MedicalRecordId);

        if (record == null)
            return NotFound("Bệnh án không tồn tại.");

        if (record.Prescription != null)
            return BadRequest("Bệnh án này đã có đơn thuốc.");

        var newPrescription = new Prescription
        {
            MedicalRecordId = dto.MedicalRecordId,
            Instructions = dto.Instructions,
            Details = dto.Details.Select(d => new PrescriptionDetail
            {
                MedicationId = d.MedicationId,
                MedicationName = d.MedicationName,
                Quantity = d.Quantity,
                Dosage = d.Dosage
            }).ToList()
        };

        _context.Prescriptions.Add(newPrescription);
        await _context.SaveChangesAsync();

        try
        {
            var pharmacyUrl = _configuration["PHARMACY_SERVICE_URL"] ?? "http://localhost:8002";
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Service-API-Key", "MedicareServiceInternalKey2024");

            int patientIntId = 5;
            if (record.Patient?.GatewayPatientId != null)
                patientIntId = record.Patient.GatewayPatientId.Value;
            else
            {
                var patient = await _context.Patients.FindAsync(record.PatientId);
                if (patient?.GatewayPatientId != null)
                    patientIntId = patient.GatewayPatientId.Value;
                else
                {
                    var lastSegment = record.PatientId.ToString().Split('-').Last().TrimStart('0');
                    if (!string.IsNullOrEmpty(lastSegment) && int.TryParse(lastSegment, out var parsed))
                        patientIntId = parsed;
                }
            }

            var payload = new
            {
                PrescriptionId = newPrescription.Id,
                PatientId = patientIntId,
                PatientName = record.Patient?.FullName ?? "Bệnh nhân",
                DoctorName = User.FindFirst("FullName")?.Value ?? "Bác sĩ",
                Medicines = dto.Details.Select(d => {
                    int medId = 1;
                    var medStr = d.MedicationId.ToString();
                    var medLast = medStr.Split('-').Last().TrimStart('0');
                    if (!string.IsNullOrEmpty(medLast) && int.TryParse(medLast, out var parsedMed))
                        medId = parsedMed;
                    return new { MedicineId = medId, MedicineName = d.MedicationName, Quantity = d.Quantity };
                }).ToList()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var response = await client.PostAsync($"{pharmacyUrl.TrimEnd('/')}/api/Prescription/create-direct",
                new StringContent(json, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
                Console.WriteLine($"✅ Created prescription bill in Pharmacy for Patient ID {patientIntId}");
            else
                Console.WriteLine($"❌ Failed to create prescription bill: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error calling Pharmacy: {ex.Message}");
        }

        return Ok(new { Message = "Chốt đơn thuốc thành công!", PrescriptionId = newPrescription.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePrescription(Guid id, [FromBody] UpdatePrescriptionDto dto)
    {
        var pres = await _context.Prescriptions
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (pres == null) return NotFound();

        var userName = User.Identity?.Name ?? "Unknown";

        if (dto.Instructions != null) pres.Instructions = dto.Instructions;
        if (dto.Status != null) pres.Status = dto.Status;
        if (dto.ExpiryDate != null) pres.ExpiryDate = dto.ExpiryDate;
        pres.UpdatedAt = DateTime.UtcNow;
        pres.UpdatedBy = userName;

        if (dto.Details != null)
        {
            _context.PrescriptionDetails.RemoveRange(pres.Details);
            pres.Details = dto.Details.Select(d => new PrescriptionDetail
            {
                PrescriptionId = pres.Id,
                MedicationId = d.MedicationId,
                MedicationName = d.MedicationName,
                Quantity = d.Quantity,
                Dosage = d.Dosage
            }).ToList();
        }

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Cập nhật đơn thuốc thành công!" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePrescription(Guid id)
    {
        var pres = await _context.Prescriptions
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (pres == null) return NotFound();
        _context.Prescriptions.Remove(pres);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã xóa đơn thuốc!" });
    }
}
