using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_de_Gestion_de_medicos.Models.Entities;

public class DoctorSchedule
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Doctor")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "El día de la semana es obligatorio")]
    [Display(Name = "Día de la Semana")]
    public DayOfWeek DayOfWeek { get; set; }

    [Required(ErrorMessage = "La hora de inicio es obligatoria")]
    [Display(Name = "Hora de Inicio")]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "La hora de fin es obligatoria")]
    [Display(Name = "Hora de Fin")]
    public TimeSpan EndTime { get; set; }

    [Display(Name = "Disponible")]
    public bool IsAvailable { get; set; } = true;

    [ForeignKey(nameof(DoctorId))]
    public Doctor Doctor { get; set; } = null!;

    [NotMapped]
    [Display(Name = "Horario")]
    public string ScheduleDisplay => $"{DayOfWeek} {StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
}
