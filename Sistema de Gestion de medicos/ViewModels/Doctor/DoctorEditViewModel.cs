using System.ComponentModel.DataAnnotations;

namespace Sistema_de_Gestion_de_medicos.ViewModels.Doctor;

public class DoctorEditViewModel
{
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
}
