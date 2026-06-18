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
    public DbSet<Icd10Code> Icd10Codes { get; set; }
    public DbSet<LabTest> LabTests { get; set; }
    public DbSet<TreatmentPlan> TreatmentPlans { get; set; }
    public DbSet<TreatmentProgression> TreatmentProgressions { get; set; }
    public DbSet<AuditLogEntry> AuditLogEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MedicalRecord>()
            .HasOne(m => m.Patient)
            .WithMany(p => p.MedicalRecords)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MedicalRecord>()
            .HasQueryFilter(m => !m.IsDeleted);

        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.MedicalRecord)
            .WithOne(m => m.Prescription)
            .HasForeignKey<Prescription>(p => p.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PrescriptionDetail>()
            .HasOne(d => d.Prescription)
            .WithMany(p => p.Details)
            .HasForeignKey(d => d.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LabTest>()
            .HasOne(l => l.MedicalRecord)
            .WithMany(m => m.LabTests)
            .HasForeignKey(l => l.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TreatmentPlan>()
            .HasOne(t => t.MedicalRecord)
            .WithMany(m => m.TreatmentPlans)
            .HasForeignKey(t => t.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TreatmentProgression>()
            .HasOne(p => p.TreatmentPlan)
            .WithMany(t => t.Progressions)
            .HasForeignKey(p => p.TreatmentPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AuditLogEntry>()
            .HasIndex(a => a.RecordId);
    }
}
