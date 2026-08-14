using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_de_Gestion_de_medicos.Models.Entities;

namespace Sistema_de_Gestion_de_medicos.Data.Configurations;

public class DoctorDailyCapacityConfiguration : IEntityTypeConfiguration<DoctorDailyCapacity>
{
    public void Configure(EntityTypeBuilder<DoctorDailyCapacity> builder)
    {
        builder.ToTable("DoctorDailyCapacities");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.DoctorId)
            .IsRequired();

        builder.Property(c => c.SpecificDate)
            .IsRequired();

        builder.Property(c => c.MaxDailyAppointments)
            .IsRequired()
            .HasDefaultValue(10);

        builder.Property(c => c.CurrentBookedAppointments)
            .HasDefaultValue(0);

        builder.Property(c => c.TimeBlock)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.IsBlocked)
            .HasDefaultValue(false);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(c => new { c.DoctorId, c.SpecificDate, c.TimeBlock })
            .IsUnique();

        builder.HasOne(c => c.Doctor)
            .WithMany(d => d.DailyCapacities)
            .HasForeignKey(c => c.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
