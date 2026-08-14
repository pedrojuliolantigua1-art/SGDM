using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_de_Gestion_de_medicos.Models.Entities;

namespace Sistema_de_Gestion_de_medicos.Data.Configurations;

public class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
{
    public void Configure(EntityTypeBuilder<DoctorSchedule> builder)
    {
        builder.ToTable("DoctorSchedules");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.DoctorId)
            .IsRequired();

        builder.Property(s => s.DayOfWeek)
            .IsRequired();

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.Property(s => s.IsAvailable)
            .HasDefaultValue(true);

        builder.HasIndex(s => new { s.DoctorId, s.DayOfWeek })
            .IsUnique()
            .HasFilter("[IsAvailable] = 1");
    }
}
