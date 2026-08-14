using System.ComponentModel.DataAnnotations;

namespace Sistema_de_Gestion_de_medicos.ViewModels.Appointment;

public class AppointmentListViewModel
{
    [Display(Name = "Fecha")]
    public DateOnly SelectedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public List<AppointmentEntry> Appointments { get; set; } = new List<AppointmentEntry>();
}

public class AppointmentEntry
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string PatientEmail { get; set; } = string.Empty;
    public string PatientPhone { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string TimeDisplay => AppointmentTime.ToString("hh\\:mm");
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay
    {
        get
        {
            return Status switch
            {
                "Scheduled" => "Programada",
                "Confirmed" => "Confirmada",
                "Cancelled" => "Cancelada",
                "Completed" => "Completada",
                "NoShow" => "No Asistió",
                _ => Status
            };
        }
    }
    public string TimeBlock { get; set; } = string.Empty;
    public string TimeBlockDisplay => TimeBlock switch
    {
        "Morning" => "Mañana",
        "Afternoon" => "Tarde",
        "Full" => "Día Completo",
        _ => TimeBlock
    };
    public string? Notes { get; set; }
    public bool CanCancel => Status != "Cancelled";
}
