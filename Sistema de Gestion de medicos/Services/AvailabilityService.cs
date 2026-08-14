using Microsoft.EntityFrameworkCore;
using Sistema_de_Gestion_de_medicos.Data;
using Sistema_de_Gestion_de_medicos.Models.Entities;

namespace Sistema_de_Gestion_de_medicos.Services;

public interface IAvailabilityService
{
    Task<IEnumerable<DoctorDailyCapacity>> GetCapacitiesByDoctorAsync(int doctorId);
    Task<DoctorDailyCapacity?> GetCapacityByDoctorAndDateAsync(int doctorId, DateOnly date, string timeBlock);
    Task<DoctorDailyCapacity> ConfigureDailyCapacityAsync(int doctorId, DoctorDailyCapacity capacity);
    Task<IEnumerable<DoctorDailyCapacity>> BulkConfigureCapacityAsync(
        int doctorId, DateOnly startDate, DateOnly endDate, int maxDailyAppointments, string timeBlock);
    Task<bool> IncrementBookedCountAsync(int doctorId, DateOnly date, string timeBlock);
    Task<bool> DecrementBookedCountAsync(int doctorId, DateOnly date, string timeBlock);
    Task<bool> ToggleBlockDayAsync(int doctorId, DateOnly date);
    Task<bool> IsDayBlockedAsync(int doctorId, DateOnly date);
    Task<bool> HasAvailableSlotsAsync(int doctorId, DateOnly date, string timeBlock);
}

public class AvailabilityService : IAvailabilityService
{
    private readonly AppDbContext _context;

    public AvailabilityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DoctorDailyCapacity>> GetCapacitiesByDoctorAsync(int doctorId)
    {
        return await _context.DoctorDailyCapacities
            .Where(c => c.DoctorId == doctorId)
            .OrderBy(c => c.SpecificDate)
            .ThenBy(c => c.TimeBlock)
            .ToListAsync();
    }

    public async Task<DoctorDailyCapacity?> GetCapacityByDoctorAndDateAsync(int doctorId, DateOnly date, string timeBlock)
    {
        return await _context.DoctorDailyCapacities
            .FirstOrDefaultAsync(c =>
                c.DoctorId == doctorId &&
                c.SpecificDate == date &&
                c.TimeBlock == timeBlock);
    }

    public async Task<DoctorDailyCapacity> ConfigureDailyCapacityAsync(int doctorId, DoctorDailyCapacity capacity)
    {
        var doctor = await _context.Doctors.FindAsync(doctorId);
        if (doctor == null)
            throw new KeyNotFoundException("Doctor no encontrado.");

        var existingCapacity = await _context.DoctorDailyCapacities
            .FirstOrDefaultAsync(c =>
                c.DoctorId == doctorId &&
                c.SpecificDate == capacity.SpecificDate &&
                c.TimeBlock == capacity.TimeBlock);

        if (existingCapacity != null)
        {
            if (capacity.MaxDailyAppointments < existingCapacity.CurrentBookedAppointments)
                throw new InvalidOperationException(
                    $"No se puede reducir el límite a {capacity.MaxDailyAppointments}. " +
                    $"Ya existen {existingCapacity.CurrentBookedAppointments} citas agendadas.");

            existingCapacity.MaxDailyAppointments = capacity.MaxDailyAppointments;
            existingCapacity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingCapacity;
        }

        capacity.DoctorId = doctorId;
        capacity.CurrentBookedAppointments = 0;
        capacity.IsBlocked = false;
        capacity.CreatedAt = DateTime.UtcNow;

        _context.DoctorDailyCapacities.Add(capacity);
        await _context.SaveChangesAsync();

        return capacity;
    }

    public async Task<IEnumerable<DoctorDailyCapacity>> BulkConfigureCapacityAsync(
        int doctorId, DateOnly startDate, DateOnly endDate, int maxDailyAppointments, string timeBlock)
    {
        var results = new List<DoctorDailyCapacity>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var newCapacity = new DoctorDailyCapacity
            {
                SpecificDate = currentDate,
                MaxDailyAppointments = maxDailyAppointments,
                TimeBlock = timeBlock
            };

            var result = await ConfigureDailyCapacityAsync(doctorId, newCapacity);
            results.Add(result);
            currentDate = currentDate.AddDays(1);
        }

        return results;
    }

    public async Task<bool> IncrementBookedCountAsync(int doctorId, DateOnly date, string timeBlock)
    {
        var capacity = await _context.DoctorDailyCapacities
            .FirstOrDefaultAsync(c =>
                c.DoctorId == doctorId &&
                c.SpecificDate == date &&
                c.TimeBlock == timeBlock);

        if (capacity == null)
        {
            capacity = new DoctorDailyCapacity
            {
                DoctorId = doctorId,
                SpecificDate = date,
                MaxDailyAppointments = 10,
                CurrentBookedAppointments = 1,
                TimeBlock = timeBlock,
                IsBlocked = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.DoctorDailyCapacities.Add(capacity);
        }
        else
        {
            if (capacity.CurrentBookedAppointments >= capacity.MaxDailyAppointments)
                return false;

            capacity.CurrentBookedAppointments++;
            capacity.IsBlocked = capacity.CurrentBookedAppointments >= capacity.MaxDailyAppointments;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DecrementBookedCountAsync(int doctorId, DateOnly date, string timeBlock)
    {
        var capacity = await _context.DoctorDailyCapacities
            .FirstOrDefaultAsync(c =>
                c.DoctorId == doctorId &&
                c.SpecificDate == date &&
                c.TimeBlock == timeBlock);

        if (capacity == null || capacity.CurrentBookedAppointments <= 0)
            return false;

        capacity.CurrentBookedAppointments--;
        capacity.IsBlocked = false;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleBlockDayAsync(int doctorId, DateOnly date)
    {
        var capacities = await _context.DoctorDailyCapacities
            .Where(c => c.DoctorId == doctorId && c.SpecificDate == date)
            .ToListAsync();

        if (!capacities.Any()) return false;

        var isCurrentlyBlocked = capacities.All(c => c.IsBlocked);

        foreach (var capacity in capacities)
        {
            capacity.IsBlocked = !isCurrentlyBlocked;
            capacity.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsDayBlockedAsync(int doctorId, DateOnly date)
    {
        return await _context.DoctorDailyCapacities
            .AnyAsync(c =>
                c.DoctorId == doctorId &&
                c.SpecificDate == date &&
                c.IsBlocked);
    }

    public async Task<bool> HasAvailableSlotsAsync(int doctorId, DateOnly date, string timeBlock)
    {
        var capacity = await _context.DoctorDailyCapacities
            .FirstOrDefaultAsync(c =>
                c.DoctorId == doctorId &&
                c.SpecificDate == date &&
                c.TimeBlock == timeBlock);

        if (capacity == null) return true;

        return capacity.CurrentBookedAppointments < capacity.MaxDailyAppointments;
    }
}
