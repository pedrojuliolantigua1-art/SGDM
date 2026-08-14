using Microsoft.AspNetCore.Mvc;
using Sistema_de_Gestion_de_medicos.Services;
using Sistema_de_Gestion_de_medicos.ViewModels.Appointment;

namespace Sistema_de_Gestion_de_medicos.Controllers;

public class AppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly IAvailabilityService _availabilityService;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        IAvailabilityService availabilityService)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _availabilityService = availabilityService;
    }

    public async Task<IActionResult> Index(DateOnly? date)
    {
        var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        var appointments = await _appointmentService.GetAppointmentsByDateAsync(selectedDate);

        var entries = appointments.Select(a => new AppointmentEntry
        {
            Id = a.Id,
            DoctorId = a.DoctorId,
            DoctorName = a.Doctor.FullName,
            PatientName = a.PatientName,
            PatientEmail = a.PatientEmail,
            PatientPhone = a.PatientPhone,
            AppointmentDate = a.AppointmentDate,
            AppointmentTime = a.AppointmentTime,
            Status = a.Status,
            TimeBlock = a.TimeBlock,
            Notes = a.Notes
        }).ToList();

        var model = new AppointmentListViewModel
        {
            SelectedDate = selectedDate,
            Appointments = entries
        };

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();
        var doctorOptions = doctors.Select(d => new DoctorOption
        {
            Id = d.Id,
            FullName = d.FullName,
            Specialty = d.Specialty
        }).ToList();

        var model = new AppointmentCreateViewModel
        {
            AvailableDoctors = doctorOptions,
            AppointmentDate = DateOnly.FromDateTime(DateTime.Today),
            TimeBlock = "Morning"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentCreateViewModel model)
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();
        model.AvailableDoctors = doctors.Select(d => new DoctorOption
        {
            Id = d.Id,
            FullName = d.FullName,
            Specialty = d.Specialty
        }).ToList();

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var appointment = new Models.Entities.Appointment
            {
                DoctorId = model.DoctorId,
                PatientName = model.PatientName,
                PatientEmail = model.PatientEmail,
                PatientPhone = model.PatientPhone,
                AppointmentDate = model.AppointmentDate,
                AppointmentTime = model.AppointmentTime,
                TimeBlock = model.TimeBlock,
                Notes = model.Notes
            };

            await _appointmentService.CreateAppointmentAsync(appointment);
            TempData["SuccessMessage"] = "Cita agendada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (KeyNotFoundException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, DateOnly? date)
    {
        try
        {
            await _appointmentService.CancelAppointmentAsync(id);
            TempData["SuccessMessage"] = "Cita cancelada exitosamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { date = date?.ToString("yyyy-MM-dd") });
    }

    [HttpGet]
    public async Task<IActionResult> CheckCapacity(int doctorId, string date, string timeBlock)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest(new { hasCapacity = false, remainingSlots = 0 });

        var hasCapacity = await _appointmentService.HasCapacityAsync(doctorId, parsedDate, timeBlock);
        var remaining = await _appointmentService.GetRemainingSlotsAsync(doctorId, parsedDate, timeBlock);

        return Json(new { hasCapacity, remainingSlots = remaining });
    }
}
