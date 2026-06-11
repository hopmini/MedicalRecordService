using MedicalRecordService.Data;
using MedicalRecordService.DTOs;
using MedicalRecordService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Configuration;

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

    [HttpPost]
    public async Task<IActionResult> CreatePrescription([FromBody] CreatePrescriptionDto dto)
    {
        // 1. Check xem bệnh án có thật không
        var record = await _context.MedicalRecords
            .Include(m => m.Prescription)
            .FirstOrDefaultAsync(m => m.Id == dto.MedicalRecordId);

        if (record == null) 
        {
            return NotFound("Bệnh án méo tồn tại, check lại ID đi m!");
        }

        // 2. Một bệnh án chỉ có 1 đơn thuốc thôi, check xem kê chưa
        if (record.Prescription != null)
        {
            return BadRequest("Bệnh án này kê đơn rồi, định cho bệnh nhân uống thuốc thay cơm à?");
        }

        // 3. Map từ DTO sang Entity
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

        // 4. Lưu vào Postgres
        _context.Prescriptions.Add(newPrescription);
        await _context.SaveChangesAsync();

        // 5. [QUAN TRỌNG] Chốt liên kết sang Pharmacy Billing Service qua Direct API
        try
        {
            var pharmacyUrl = _configuration["PHARMACY_SERVICE_URL"] ?? "http://localhost:8002";
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Service-API-Key", "MedicareServiceInternalKey2024");

            // Get Gateway Patient ID from Patient model (fallback to parsing from GUID)
            int patientIntId = 5;
            if (record.Patient != null && record.Patient.GatewayPatientId.HasValue)
            {
                patientIntId = record.Patient.GatewayPatientId.Value;
            }
            else
            {
                var patient = await _context.Patients.FindAsync(record.PatientId);
                if (patient?.GatewayPatientId != null)
                {
                    patientIntId = patient.GatewayPatientId.Value;
                }
                else
                {
                    var guidString = record.PatientId.ToString();
                    var lastSegment = guidString.Split('-').Last().TrimStart('0');
                    if (!string.IsNullOrEmpty(lastSegment) && int.TryParse(lastSegment, out int parsedInt))
                    {
                        patientIntId = parsedInt;
                    }
                }
            }

            var payload = new
            {
                PrescriptionId = newPrescription.Id,
                PatientId = patientIntId,
                DoctorName = User.FindFirst("FullName")?.Value ?? User.FindFirst("fullName")?.Value ?? "Bác sĩ",
                Medicines = dto.Details.Select(d => {
                    int medicineIntId = 1;
                    var medStr = d.MedicationId.ToString();
                    if (!string.IsNullOrEmpty(medStr))
                    {
                        var medLastSegment = medStr.Split('-').Last().TrimStart('0');
                        if (!string.IsNullOrEmpty(medLastSegment) && int.TryParse(medLastSegment, out int parsedMedId))
                        {
                            medicineIntId = parsedMedId;
                        }
                        else if (int.TryParse(medStr, out int parsedDirect))
                        {
                            medicineIntId = parsedDirect;
                        }
                    }
                    return new
                    {
                        MedicineId = medicineIntId,
                        MedicineName = d.MedicationName,
                        Quantity = d.Quantity
                    };
                }).ToList()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{pharmacyUrl.TrimEnd('/')}/api/Prescription/create-direct", content);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Successfully created pending prescription bill in Pharmacy Service for Patient ID {patientIntId}");
            }
            else
            {
                var errMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Failed to create prescription bill in Pharmacy Service: {response.StatusCode} - {errMsg}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error calling Pharmacy Billing Service: {ex.Message}");
        }

        Console.WriteLine($"🚀 [EVENT PUBLISHED] prescription.created: Đã kê đơn {newPrescription.Id} cho bệnh án {record.Id}");

        return Ok(new { 
            Message = "Chốt đơn thuốc thành công!", 
            PrescriptionId = newPrescription.Id 
        });
    }
}