using MedicalRecordService.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalRecordService.Data;

public class MedicalDbContext : DbContext
{
    public MedicalDbContext(DbContextOptions<MedicalDbContext> options) : base(options) { }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Config quan hệ giữa các bảng cho đỡ bị ngáo lúc join
        modelBuilder.Entity<MedicalRecord>()
            .HasOne(m => m.Patient)
            .WithMany(p => p.MedicalRecords)
            .HasForeignKey(m => m.PatientId);

        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.MedicalRecord)
            .WithOne(m => m.Prescription)
            .HasForeignKey<Prescription>(p => p.MedicalRecordId);

        modelBuilder.Entity<PrescriptionDetail>()
            .HasOne(d => d.Prescription)
            .WithMany(p => p.Details)
            .HasForeignKey(d => d.PrescriptionId);
    }
}