using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_de_Gestion_de_medicos.Models.Entities;

namespace Sistema_de_Gestion_de_medicos.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Specialty)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(d => d.Email)
            .IsUnique();

        builder.Property(d => d.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(d => d.IsActive)
            .HasDefaultValue(true);

        builder.Property(d => d.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasMany(d => d.Schedules)
            .WithOne(s => s.Doctor)
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.DailyCapacities)
            .WithOne(c => c.Doctor)
            .HasForeignKey(c => c.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Appointments)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
