using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_de_Gestion_de_medicos.Models.Entities;

public class Doctor
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100)]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [MaxLength(100)]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especialidad es obligatoria")]
    [MaxLength(150)]
    [Display(Name = "Especialidad")]
    public string Specialty { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    [MaxLength(150)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [Phone(ErrorMessage = "Formato de teléfono inválido")]
    [MaxLength(20)]
    [Display(Name = "Teléfono")]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Dirección")]
    public string? Address { get; set; }

    [MaxLength(1000)]
    [Display(Name = "Biografía")]
    public string? Biography { get; set; }

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Fecha de Creación")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Última Actualización")]
    public DateTime? UpdatedAt { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    [NotMapped]
    [Display(Name = "Nombre Completo")]
    public string FullName => $"{FirstName} {LastName}";

    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
    public ICollection<DoctorDailyCapacity> DailyCapacities { get; set; } = new List<DoctorDailyCapacity>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
