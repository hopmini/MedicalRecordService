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
public class LabTestsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public LabTestsController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> UpdateLabTest(Guid id, [FromBody] UpdateLabTestDto dto)
    {
        var labTest = await _context.LabTests.FindAsync(id);
        if (labTest == null) return NotFound("Không tìm thấy xét nghiệm.");

        if (dto.Result != null) labTest.Result = dto.Result;
        if (dto.Notes != null) labTest.Notes = dto.Notes;
        if (dto.AttachmentFileJson != null) labTest.AttachmentFileJson = dto.AttachmentFileJson;
        if (dto.PerformedBy != null) labTest.PerformedBy = dto.PerformedBy;
        if (dto.CompletedAt != null) labTest.CompletedAt = dto.CompletedAt;

        if (dto.Status != null && Enum.TryParse<LabTestStatus>(dto.Status, true, out var newStatus))
        {
            labTest.Status = newStatus;
            if (newStatus == LabTestStatus.Completed && labTest.CompletedAt == null)
                labTest.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Cập nhật kết quả xét nghiệm thành công." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> DeleteLabTest(Guid id)
    {
        var labTest = await _context.LabTests.FindAsync(id);
        if (labTest == null) return NotFound();
        _context.LabTests.Remove(labTest);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã xóa xét nghiệm." });
    }
}
