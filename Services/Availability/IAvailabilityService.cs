namespace ReserveFlow.Services.Availability;

public interface IAvailabilityService
{
    /// <summary>
    /// Calculates each day's availability for a resource and month.
    /// </summary>
    Task<IReadOnlyList<DailyAvailability>> GetMonthAsync(
        Guid resourceId,
        int year,
        int month,
        CancellationToken cancellationToken = default);
}