using MedicalRecordService.Data;
using MedicalRecordService.DTOs;
using MedicalRecordService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalRecordService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public PrescriptionsController(MedicalDbContext context)
    {
        _context = context;
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

        // 5. [QUAN TRỌNG] Chỗ này mốt cài Message Broker (RabbitMQ/Kafka) thì code logic bắn message ở đây
        // Bắn cái "prescription.created" sang cho con PharmacyService N3
        Console.WriteLine($"🚀 [EVENT PUBLISHED] prescription.created: Đã kê đơn {newPrescription.Id} cho bệnh án {record.Id}");

        return Ok(new { 
            Message = "Chốt đơn thuốc thành công!", 
            PrescriptionId = newPrescription.Id 
        });
    }
}