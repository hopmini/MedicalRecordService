using MedicalRecordService.Data;
using MedicalRecordService.DTOs;
using MedicalRecordService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalRecordService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TreatmentPlansController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public TreatmentPlansController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> UpdateTreatmentPlan(Guid id, [FromBody] UpdateTreatmentPlanDto dto)
    {
        var plan = await _context.TreatmentPlans.FindAsync(id);
        if (plan == null) return NotFound("Không tìm thấy phác đồ điều trị.");

        if (dto.PlanName != null) plan.PlanName = dto.PlanName;
        if (dto.Description != null) plan.Description = dto.Description;
        if (dto.StartDate != null) plan.StartDate = dto.StartDate.Value;
        if (dto.EndDate != null) plan.EndDate = dto.EndDate;
        if (dto.Status != null && Enum.TryParse<TreatmentPlanStatus>(dto.Status, true, out var newStatus))
            plan.Status = newStatus;
        plan.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Cập nhật phác đồ điều trị thành công." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> DeleteTreatmentPlan(Guid id)
    {
        var plan = await _context.TreatmentPlans.FindAsync(id);
        if (plan == null) return NotFound();
        _context.TreatmentPlans.Remove(plan);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã xóa phác đồ điều trị." });
    }

    [HttpPost("{planId}/progressions")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> AddProgression(Guid planId, [FromBody] CreateProgressionDto dto)
    {
        var plan = await _context.TreatmentPlans.FindAsync(planId);
        if (plan == null) return NotFound("Không tìm thấy phác đồ điều trị.");

        var progression = new TreatmentProgression
        {
            TreatmentPlanId = planId,
            Notes = dto.Notes,
            Status = dto.Status,
            RecordedBy = User.Identity?.Name ?? "Unknown"
        };
        _context.TreatmentProgressions.Add(progression);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã ghi nhận diễn biến điều trị.", ProgressionId = progression.Id });
    }
}
