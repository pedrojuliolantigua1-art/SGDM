using Microsoft.AspNetCore.Mvc;
using Sistema_de_Gestion_de_medicos.Models.Entities;
using Sistema_de_Gestion_de_medicos.Services;
using Sistema_de_Gestion_de_medicos.ViewModels.Doctor;

namespace Sistema_de_Gestion_de_medicos.Controllers;

public class DoctorsController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IAvailabilityService _availabilityService;

    public DoctorsController(IDoctorService doctorService, IAvailabilityService availabilityService)
    {
        _doctorService = doctorService;
        _availabilityService = availabilityService;
    }

    public async Task<IActionResult> Index()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();
        return View(doctors);
    }

    public IActionResult Create()
    {
        return View(new DoctorCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DoctorCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var doctor = new Doctor
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Specialty = model.Specialty,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                Biography = model.Biography
            };

            await _doctorService.CreateDoctorAsync(doctor);
            TempData["SuccessMessage"] = "Doctor registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(id);
        if (doctor == null) return NotFound();

        var model = new DoctorEditViewModel
        {
            Id = doctor.Id,
            FirstName = doctor.FirstName,
            LastName = doctor.LastName,
            Specialty = doctor.Specialty,
            Phone = doctor.Phone,
            Address = doctor.Address,
            Biography = doctor.Biography,
            IsActive = doctor.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DoctorEditViewModel model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var doctor = new Doctor
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Specialty = model.Specialty,
            Phone = model.Phone,
            Address = model.Address,
            Biography = model.Biography,
            IsActive = model.IsActive
        };

        var updated = await _doctorService.UpdateDoctorAsync(id, doctor);
        if (updated == null) return NotFound();

        TempData["SuccessMessage"] = "Doctor actualizado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var toggled = await _doctorService.ToggleDoctorStatusAsync(id);
        if (!toggled) return NotFound();

        TempData["SuccessMessage"] = "Estado del doctor actualizado.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Schedule(int id)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(id);
        if (doctor == null) return NotFound();

        var schedules = await _doctorService.GetDoctorSchedulesAsync(id);
        var scheduleEntries = schedules.Select(s => new ScheduleEntry
        {
            Id = s.Id,
            DayOfWeek = (int)s.DayOfWeek,
            DayName = s.DayOfWeek.ToString(),
            StartTime = s.StartTime,
            EndTime = s.EndTime
        }).ToList();

        var model = new DoctorScheduleViewModel
        {
            DoctorId = doctor.Id,
            DoctorName = doctor.FullName,
            Specialty = doctor.Specialty,
            Schedules = scheduleEntries
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSchedule(DoctorScheduleViewModel model)
    {
        try
        {
            var schedule = new DoctorSchedule
            {
                DayOfWeek = (DayOfWeek)model.DayOfWeek,
                StartTime = model.StartTime,
                EndTime = model.EndTime
            };

            await _doctorService.AddDoctorScheduleAsync(model.DoctorId, schedule);
            TempData["SuccessMessage"] = "Horario agregado exitosamente.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Schedule), new { id = model.DoctorId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveSchedule(int scheduleId, int doctorId)
    {
        var removed = await _doctorService.RemoveDoctorScheduleAsync(scheduleId);
        if (!removed) return NotFound();

        TempData["SuccessMessage"] = "Horario eliminado.";
        return RedirectToAction(nameof(Schedule), new { id = doctorId });
    }

    public async Task<IActionResult> Capacity(int id)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(id);
        if (doctor == null) return NotFound();

        var capacities = await _availabilityService.GetCapacitiesByDoctorAsync(id);
        var capacityEntries = capacities.Select(c => new CapacityEntry
        {
            Id = c.Id,
            SpecificDate = c.SpecificDate,
            MaxDailyAppointments = c.MaxDailyAppointments,
            CurrentBookedAppointments = c.CurrentBookedAppointments,
            TimeBlock = c.TimeBlock,
            IsBlocked = c.IsBlocked
        }).ToList();

        var model = new DoctorCapacityViewModel
        {
            DoctorId = doctor.Id,
            DoctorName = doctor.FullName,
            Specialty = doctor.Specialty,
            Capacities = capacityEntries
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfigureCapacity(DoctorCapacityViewModel model)
    {
        try
        {
            if (model.StartDate > model.EndDate)
            {
                TempData["ErrorMessage"] = "La fecha de inicio debe ser menor a la fecha de fin.";
                return RedirectToAction(nameof(Capacity), new { id = model.DoctorId });
            }

            await _availabilityService.BulkConfigureCapacityAsync(
                model.DoctorId,
                model.StartDate,
                model.EndDate,
                model.MaxDailyAppointments,
                model.TimeBlock);

            TempData["SuccessMessage"] = "Capacidad configurada exitosamente.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Capacity), new { id = model.DoctorId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(int doctorId, DateOnly date)
    {
        var toggled = await _availabilityService.ToggleBlockDayAsync(doctorId, date);
        if (!toggled) return NotFound();

        TempData["SuccessMessage"] = "Estado de bloqueo actualizado.";
        return RedirectToAction(nameof(Capacity), new { id = doctorId });
    }
}
