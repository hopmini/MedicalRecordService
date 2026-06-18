using System.ComponentModel.DataAnnotations;

namespace MedicalRecordService.Models;

public class Icd10Code
{
    [Key]
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Category { get; set; }
}
