using System.ComponentModel.DataAnnotations;

namespace Sistema_de_Gestion_de_medicos.ViewModels.Doctor;

public class DoctorCapacityViewModel
{
    public int DoctorId { get; set; }

    [Display(Name = "Nombre del Doctor")]
    public string DoctorName { get; set; } = string.Empty;

    [Display(Name = "Especialidad")]
    public string Specialty { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
    [Display(Name = "Fecha de Inicio")]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "La fecha de fin es obligatoria")]
    [Display(Name = "Fecha de Fin")]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

    [Required(ErrorMessage = "El límite máximo de citas es obligatorio")]
    [Range(1, 200, ErrorMessage = "El límite debe estar entre 1 y 200")]
    [Display(Name = "Máximo de Citas por Día")]
    public int MaxDailyAppointments { get; set; } = 10;

    [Required(ErrorMessage = "El bloque horario es obligatorio")]
    [Display(Name = "Bloque Horario")]
    public string TimeBlock { get; set; } = "Morning";

    public List<CapacityEntry> Capacities { get; set; } = new List<CapacityEntry>();
}

public class CapacityEntry
{
    public int Id { get; set; }
    public DateOnly SpecificDate { get; set; }
    public string DateDisplay => SpecificDate.ToString("dd/MM/yyyy");
    public int MaxDailyAppointments { get; set; }
    public int CurrentBookedAppointments { get; set; }
    public int RemainingSlots => Math.Max(0, MaxDailyAppointments - CurrentBookedAppointments);
    public string TimeBlock { get; set; } = string.Empty;
    public string TimeBlockDisplay => TimeBlock switch
    {
        "Morning" => "Mañana",
        "Afternoon" => "Tarde",
        "Full" => "Día Completo",
        _ => TimeBlock
    };
    public bool IsBlocked { get; set; }
    public bool HasReachedCapacity => CurrentBookedAppointments >= MaxDailyAppointments;
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
