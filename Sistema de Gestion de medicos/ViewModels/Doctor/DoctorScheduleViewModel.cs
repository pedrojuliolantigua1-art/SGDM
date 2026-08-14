using System.ComponentModel.DataAnnotations;

namespace Sistema_de_Gestion_de_medicos.ViewModels.Doctor;

public class DoctorScheduleViewModel
{
    public int DoctorId { get; set; }

    [Display(Name = "Nombre del Doctor")]
    public string DoctorName { get; set; } = string.Empty;

    [Display(Name = "Especialidad")]
    public string Specialty { get; set; } = string.Empty;

    [Required(ErrorMessage = "El día de la semana es obligatorio")]
    [Display(Name = "Día de la Semana")]
    public int DayOfWeek { get; set; }

    [Required(ErrorMessage = "La hora de inicio es obligatoria")]
    [Display(Name = "Hora de Inicio")]
    public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0);

    [Required(ErrorMessage = "La hora de fin es obligatoria")]
    [Display(Name = "Hora de Fin")]
    public TimeSpan EndTime { get; set; } = new TimeSpan(12, 0, 0);

    public List<ScheduleEntry> Schedules { get; set; } = new List<ScheduleEntry>();
}

public class ScheduleEntry
{
    public int Id { get; set; }
    public int DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string ScheduleDisplay => $"{DayName} {StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
}
