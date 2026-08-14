using Microsoft.EntityFrameworkCore;
using Sistema_de_Gestion_de_medicos.Data;
using Sistema_de_Gestion_de_medicos.Models.Entities;

namespace Sistema_de_Gestion_de_medicos.Services;

public interface IDoctorService
{
    Task<IEnumerable<Doctor>> GetAllDoctorsAsync();
    Task<Doctor?> GetDoctorByIdAsync(int id);
    Task<Doctor> CreateDoctorAsync(Doctor doctor);
    Task<Doctor?> UpdateDoctorAsync(int id, Doctor doctor);
    Task<bool> DeleteDoctorAsync(int id);
    Task<bool> ToggleDoctorStatusAsync(int id);
    Task<IEnumerable<DoctorSchedule>> GetDoctorSchedulesAsync(int doctorId);
    Task<DoctorSchedule> AddDoctorScheduleAsync(int doctorId, DoctorSchedule schedule);
    Task<bool> RemoveDoctorScheduleAsync(int scheduleId);
}

public class DoctorService : IDoctorService
{
    private readonly AppDbContext _context;

    public DoctorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync()
    {
        return await _context.Doctors
            .Where(d => d.IsActive)
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .ToListAsync();
    }

    public async Task<Doctor?> GetDoctorByIdAsync(int id)
    {
        return await _context.Doctors.FindAsync(id);
    }

    public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
    {
        var existingDoctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Email == doctor.Email);

        if (existingDoctor != null)
            throw new InvalidOperationException("Ya existe un doctor registrado con este email.");

        doctor.CreatedAt = DateTime.UtcNow;
        doctor.IsActive = true;

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        return doctor;
    }

    public async Task<Doctor?> UpdateDoctorAsync(int id, Doctor doctor)
    {
        var existingDoctor = await _context.Doctors.FindAsync(id);
        if (existingDoctor == null) return null;

        existingDoctor.FirstName = doctor.FirstName;
        existingDoctor.LastName = doctor.LastName;
        existingDoctor.Specialty = doctor.Specialty;
        existingDoctor.Phone = doctor.Phone;
        existingDoctor.Address = doctor.Address;
        existingDoctor.Biography = doctor.Biography;
        existingDoctor.IsActive = doctor.IsActive;
        existingDoctor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingDoctor;
    }

    public async Task<bool> DeleteDoctorAsync(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null) return false;

        doctor.IsActive = false;
        doctor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleDoctorStatusAsync(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null) return false;

        doctor.IsActive = !doctor.IsActive;
        doctor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<DoctorSchedule>> GetDoctorSchedulesAsync(int doctorId)
    {
        return await _context.DoctorSchedules
            .Where(s => s.DoctorId == doctorId && s.IsAvailable)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync();
    }

    public async Task<DoctorSchedule> AddDoctorScheduleAsync(int doctorId, DoctorSchedule schedule)
    {
        var doctor = await _context.Doctors.FindAsync(doctorId);
        if (doctor == null)
            throw new KeyNotFoundException("Doctor no encontrado.");

        if (schedule.StartTime >= schedule.EndTime)
            throw new ArgumentException("La hora de inicio debe ser menor a la hora de fin.");

        var existingSchedule = await _context.DoctorSchedules
            .FirstOrDefaultAsync(s =>
                s.DoctorId == doctorId &&
                s.DayOfWeek == schedule.DayOfWeek &&
                s.IsAvailable);

        if (existingSchedule != null)
            throw new InvalidOperationException("Ya existe un horario registrado para este día.");

        schedule.DoctorId = doctorId;
        schedule.IsAvailable = true;

        _context.DoctorSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return schedule;
    }

    public async Task<bool> RemoveDoctorScheduleAsync(int scheduleId)
    {
        var schedule = await _context.DoctorSchedules.FindAsync(scheduleId);
        if (schedule == null) return false;

        schedule.IsAvailable = false;
        await _context.SaveChangesAsync();
        return true;
    }
}
