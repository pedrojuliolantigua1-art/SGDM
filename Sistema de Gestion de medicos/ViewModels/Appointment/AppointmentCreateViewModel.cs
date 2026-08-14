using System.ComponentModel.DataAnnotations;

namespace Sistema_de_Gestion_de_medicos.ViewModels.Appointment;

public class AppointmentCreateViewModel
{
    [Required(ErrorMessage = "El doctor es obligatorio")]
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
    public DateOnly AppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "La hora de la cita es obligatoria")]
    [Display(Name = "Hora de la Cita")]
    public TimeSpan AppointmentTime { get; set; }

    [Required(ErrorMessage = "El bloque horario es obligatorio")]
    [Display(Name = "Bloque Horario")]
    public string TimeBlock { get; set; } = "Morning";

    [MaxLength(1000)]
    [Display(Name = "Notas")]
    public string? Notes { get; set; }

    public List<DoctorOption> AvailableDoctors { get; set; } = new List<DoctorOption>();
    public bool HasCapacity { get; set; } = true;
    public int RemainingSlots { get; set; } = 10;
}

public class DoctorOption
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
}
