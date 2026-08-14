using Microsoft.EntityFrameworkCore;
using Sistema_de_Gestion_de_medicos.Data;
using Sistema_de_Gestion_de_medicos.Models.Entities;

namespace Sistema_de_Gestion_de_medicos.Services;

public interface IAppointmentService
{
    Task<Appointment?> CreateAppointmentAsync(Appointment appointment);
    Task<Appointment?> GetAppointmentByIdAsync(int id);
    Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAndDateAsync(int doctorId, DateOnly date);
    Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateOnly date);
    Task<Appointment?> CancelAppointmentAsync(int appointmentId);
    Task<bool> HasCapacityAsync(int doctorId, DateOnly date, string timeBlock);
    Task<int> GetRemainingSlotsAsync(int doctorId, DateOnly date, string timeBlock);
}

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _context;
    private readonly IAvailabilityService _availabilityService;

    public AppointmentService(AppDbContext context, IAvailabilityService availabilityService)
    {
        _context = context;
        _availabilityService = availabilityService;
    }

    public async Task<Appointment?> CreateAppointmentAsync(Appointment appointment)
    {
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == appointment.DoctorId && d.IsActive);

        if (doctor == null)
            throw new KeyNotFoundException("El doctor seleccionado no existe o no está activo.");

        if (appointment.AppointmentDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("No se pueden agendar citas en fechas pasadas.");

        if (string.IsNullOrEmpty(appointment.TimeBlock))
            appointment.TimeBlock = appointment.AppointmentTime < new TimeSpan(12, 0, 0) ? "Morning" : "Afternoon";

        var isBlocked = await _availabilityService.IsDayBlockedAsync(appointment.DoctorId, appointment.AppointmentDate);
        if (isBlocked)
            throw new InvalidOperationException(
                $"El día {appointment.AppointmentDate:dd/MM/yyyy} está bloqueado para el Dr. {doctor.FirstName} {doctor.LastName}. Capacidad máxima alcanzada.");

        var hasAvailableSlots = await _availabilityService.HasAvailableSlotsAsync(
            appointment.DoctorId, appointment.AppointmentDate, appointment.TimeBlock);

        if (!hasAvailableSlots)
            throw new InvalidOperationException(
                $"No hay disponibilidad para el bloque {appointment.TimeBlock} del día {appointment.AppointmentDate:dd/MM/yyyy}. " +
                $"Ha alcanzado el límite máximo de citas permitidas.");

        var existingAppointment = await _context.Appointments
            .FirstOrDefaultAsync(a =>
                a.DoctorId == appointment.DoctorId &&
                a.AppointmentDate == appointment.AppointmentDate &&
                a.AppointmentTime == appointment.AppointmentTime &&
                a.Status != AppointmentStatus.Cancelled);

        if (existingAppointment != null)
            throw new InvalidOperationException(
                $"El doctor ya tiene una cita programada a las {appointment.AppointmentTime:hh\\:mm} del día {appointment.AppointmentDate:dd/MM/yyyy}.");

        appointment.Status = AppointmentStatus.Scheduled;
        appointment.CreatedAt = DateTime.UtcNow;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var incremented = await _availabilityService.IncrementBookedCountAsync(
                appointment.DoctorId, appointment.AppointmentDate, appointment.TimeBlock);

            if (!incremented)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException(
                    "No se pudo incrementar el contador de citas. La capacidad máxima fue alcanzada.");
            }

            await transaction.CommitAsync();
            return appointment;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAndDateAsync(int doctorId, DateOnly date)
    {
        return await _context.Appointments
            .Include(a => a.Doctor)
            .Where(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate == date &&
                a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateOnly date)
    {
        return await _context.Appointments
            .Include(a => a.Doctor)
            .Where(a =>
                a.AppointmentDate == date &&
                a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<Appointment?> CancelAppointmentAsync(int appointmentId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null) return null;

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("La cita ya está cancelada.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _availabilityService.DecrementBookedCountAsync(
                appointment.DoctorId, appointment.AppointmentDate, appointment.TimeBlock);

            await transaction.CommitAsync();
            return appointment;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> HasCapacityAsync(int doctorId, DateOnly date, string timeBlock)
    {
        return await _availabilityService.HasAvailableSlotsAsync(doctorId, date, timeBlock);
    }

    public async Task<int> GetRemainingSlotsAsync(int doctorId, DateOnly date, string timeBlock)
    {
        var capacity = await _context.DoctorDailyCapacities
            .FirstOrDefaultAsync(c =>
                c.DoctorId == doctorId &&
                c.SpecificDate == date &&
                c.TimeBlock == timeBlock);

        if (capacity == null) return 10;

        return Math.Max(0, capacity.MaxDailyAppointments - capacity.CurrentBookedAppointments);
    }
}
