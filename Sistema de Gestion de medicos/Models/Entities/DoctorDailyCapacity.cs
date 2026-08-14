using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_de_Gestion_de_medicos.Models.Entities;

public class DoctorDailyCapacity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Doctor")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria")]
    [Display(Name = "Fecha Específica")]
    public DateOnly SpecificDate { get; set; }

    [Required(ErrorMessage = "El límite máximo de citas es obligatorio")]
    [Range(1, 200, ErrorMessage = "El límite debe estar entre 1 y 200")]
    [Display(Name = "Máximo de Citas Diarias")]
    public int MaxDailyAppointments { get; set; }

    [Display(Name = "Citas Agendadas Actualmente")]
    public int CurrentBookedAppointments { get; set; } = 0;

    [Required(ErrorMessage = "El bloque horario es obligatorio")]
    [MaxLength(20)]
    [Display(Name = "Bloque Horario")]
    public string TimeBlock { get; set; } = string.Empty;

    [Display(Name = "Bloqueado")]
    public bool IsBlocked { get; set; } = false;

    [Display(Name = "Fecha de Creación")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Última Actualización")]
    public DateTime? UpdatedAt { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    [ForeignKey(nameof(DoctorId))]
    public Doctor Doctor { get; set; } = null!;

    [NotMapped]
    [Display(Name = "Capacidad Máxima Alcanzada")]
    public bool HasReachedCapacity => CurrentBookedAppointments >= MaxDailyAppointments;

    [NotMapped]
    [Display(Name = "Espacios Restantes")]
    public int RemainingSlots => Math.Max(0, MaxDailyAppointments - CurrentBookedAppointments);

    [NotMapped]
    [Display(Name = "Estado")]
    public string StatusDisplay
    {
        get
        {
            if (IsBlocked) return "Bloqueado";
            if (HasReachedCapacity) return "Capacidad Máxima";
            return "Disponible";
        }
    }
}
