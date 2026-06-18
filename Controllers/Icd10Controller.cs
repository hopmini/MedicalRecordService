using MedicalRecordService.Data;
using MedicalRecordService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalRecordService.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class Icd10Controller : ControllerBase
{
    private readonly MedicalDbContext _context;

    public Icd10Controller(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Icd10Code>>> GetAll([FromQuery] string? q = null)
    {
        var query = _context.Icd10Codes.AsQueryable();
        if (!string.IsNullOrEmpty(q))
        {
            var lower = q.ToLower();
            query = query.Where(c => c.Code.ToLower().Contains(lower) || c.Name.ToLower().Contains(lower));
        }
        return await query.OrderBy(c => c.Code).Take(100).ToListAsync();
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        var categories = await _context.Icd10Codes.Select(c => c.Category).Distinct().OrderBy(x => x).ToListAsync();
        return Ok(categories.Where(c => !string.IsNullOrEmpty(c)).ToList()!);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Icd10Code dto)
    {
        if (string.IsNullOrEmpty(dto.Code) || string.IsNullOrEmpty(dto.Name))
            return BadRequest("Mã và tên không được để trống.");
        var exists = await _context.Icd10Codes.AnyAsync(c => c.Code == dto.Code);
        if (exists) return BadRequest($"Mã {dto.Code} đã tồn tại.");
        var entity = new Icd10Code { Code = dto.Code, Name = dto.Name, Category = dto.Category ?? "Khác" };
        _context.Icd10Codes.Add(entity);
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Icd10Code dto)
    {
        var entity = await _context.Icd10Codes.FindAsync(id);
        if (entity == null) return NotFound();
        if (!string.IsNullOrEmpty(dto.Code)) entity.Code = dto.Code;
        if (!string.IsNullOrEmpty(dto.Name)) entity.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Category)) entity.Category = dto.Category;
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Icd10Codes.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Icd10Codes.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã xóa mã ICD-10." });
    }
}
