using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_de_Gestion_de_medicos.Models.Entities;

public class Appointment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Doctor")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "El nombre del paciente es obligatorio")]
    [MaxLength(150)]
    [Display(Name = "Nombre del Paciente")]
    public string PatientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email del paciente es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    [MaxLength(120)]
    [Display(Name = "Email del Paciente")]
    public string PatientEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono del paciente es obligatorio")]
    [Phone(ErrorMessage = "Formato de teléfono inválido")]
    [MaxLength(20)]
    [Display(Name = "Teléfono del Paciente")]
    public string PatientPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de la cita es obligatoria")]
    [Display(Name = "Fecha de la Cita")]
    public DateOnly AppointmentDate { get; set; }

    [Required(ErrorMessage = "La hora de la cita es obligatoria")]
    [Display(Name = "Hora de la Cita")]
    public TimeSpan AppointmentTime { get; set; }

    [Required]
    [MaxLength(30)]
    [Display(Name = "Estado")]
    public string Status { get; set; } = AppointmentStatus.Scheduled;

    [Required]
    [MaxLength(20)]
    [Display(Name = "Bloque Horario")]
    public string TimeBlock { get; set; } = string.Empty;

    [MaxLength(1000)]
    [Display(Name = "Notas")]
    public string? Notes { get; set; }

    [Display(Name = "Fecha de Creación")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Última Actualización")]
    public DateTime? UpdatedAt { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    [ForeignKey(nameof(DoctorId))]
    public Doctor Doctor { get; set; } = null!;

    [NotMapped]
    [Display(Name = "Estado de la Cita")]
    public string StatusDisplay
    {
        get
        {
            return Status switch
            {
                AppointmentStatus.Scheduled => "Programada",
                AppointmentStatus.Confirmed => "Confirmada",
                AppointmentStatus.Cancelled => "Cancelada",
                AppointmentStatus.Completed => "Completada",
                AppointmentStatus.NoShow => "No Asistió",
                _ => Status
            };
        }
    }

    [NotMapped]
    [Display(Name = "Bloque Horario")]
    public string TimeBlockDisplay
    {
        get
        {
            return TimeBlock switch
            {
                "Morning" => "Mañana (08:00 - 12:00)",
                "Afternoon" => "Tarde (13:00 - 17:00)",
                "Full" => "Día Completo",
                _ => TimeBlock
            };
        }
    }
}

public static class AppointmentStatus
{
    public const string Scheduled = "Scheduled";
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";
    public const string Completed = "Completed";
    public const string NoShow = "NoShow";
}
