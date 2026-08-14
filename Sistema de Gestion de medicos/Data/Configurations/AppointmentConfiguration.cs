using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_de_Gestion_de_medicos.Models.Entities;

namespace Sistema_de_Gestion_de_medicos.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.DoctorId)
            .IsRequired();

        builder.Property(a => a.PatientName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.PatientEmail)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(a => a.PatientPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.AppointmentDate)
            .IsRequired();

        builder.Property(a => a.AppointmentTime)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue(AppointmentStatus.Scheduled);

        builder.Property(a => a.TimeBlock)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.AppointmentTime })
            .IsUnique()
            .HasFilter("[Status] != 'Cancelled'");

        builder.HasIndex(a => a.AppointmentDate);

        builder.HasIndex(a => a.Status);

        builder.HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
