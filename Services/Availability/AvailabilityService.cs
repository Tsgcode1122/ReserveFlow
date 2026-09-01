using Microsoft.EntityFrameworkCore;
using ReserveFlow.Data;
using ReserveFlow.Data.Enums;

namespace ReserveFlow.Services.Availability;

public sealed class AvailabilityService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory)
    : IAvailabilityService
{
    private readonly IDbContextFactory<ApplicationDbContext>
        _dbContextFactory = dbContextFactory;

    /// <summary>
    /// Combines weekly operating hours and active reservations to
    /// calculate the resource's status for every day in a month.
    /// </summary>
    public async Task<IReadOnlyList<DailyAvailability>> GetMonthAsync(
        Guid resourceId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(
                nameof(month),
                "Month must be between 1 and 12.");
        }

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var resource = await dbContext.Resources
            .AsNoTracking()
            .Include(resource => resource.Location)
            .Include(resource => resource.OperatingHours)
            .SingleOrDefaultAsync(
                resource => resource.Id == resourceId,
                cancellationToken);

        if (resource is null)
        {
            throw new KeyNotFoundException(
                "The requested resource could not be found.");
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
            resource.Location.TimeZoneId);

        var monthStart = new DateOnly(year, month, 1);
        var monthEnd = monthStart.AddMonths(1);

        // Convert the local month boundaries to UTC before querying
        // PostgreSQL, where reservation timestamps are stored in UTC.
        var monthStartUtc = ConvertToUtc(
            monthStart,
            TimeOnly.MinValue,
            timeZone);

        var monthEndUtc = ConvertToUtc(
            monthEnd,
            TimeOnly.MinValue,
            timeZone);

        var blockingStatuses = new[]
        {
            ReservationStatus.Pending,
            ReservationStatus.Confirmed,
            ReservationStatus.CheckedIn
        };

        // Load only reservations that overlap the requested month.
        var reservations = await dbContext.Reservations
            .AsNoTracking()
            .Where(reservation =>
                reservation.ResourceId == resourceId &&
                blockingStatuses.Contains(reservation.Status) &&
                reservation.StartTime < monthEndUtc &&
                monthStartUtc < reservation.EndTime)
            .ToListAsync(cancellationToken);

        var results = new List<DailyAvailability>();

        for (var date = monthStart; date < monthEnd; date = date.AddDays(1))
        {
            var schedule = resource.OperatingHours
                .SingleOrDefault(schedule =>
                    schedule.DayOfWeek == date.DayOfWeek);

            if (schedule is null || schedule.IsClosed)
            {
                results.Add(new DailyAvailability(
                    date,
                    DailyAvailabilityStatus.Closed));

                continue;
            }

            var openingTimeUtc = ConvertToUtc(
                date,
                schedule.OpenTime,
                timeZone);

            var closingTimeUtc = ConvertToUtc(
                date,
                schedule.CloseTime,
                timeZone);

            var dailyReservations = reservations
                .Where(reservation =>
                    reservation.StartTime < closingTimeUtc &&
                    openingTimeUtc < reservation.EndTime)
                .ToList();

            if (dailyReservations.Count == 0)
            {
                results.Add(new DailyAvailability(
                    date,
                    DailyAvailabilityStatus.Available));

                continue;
            }

            var operatingMinutes =
                (closingTimeUtc - openingTimeUtc).TotalMinutes;

            // Reservations are clipped to operating hours so time outside
            // the daily schedule does not affect availability.
            var reservedMinutes = dailyReservations.Sum(reservation =>
            {
                var effectiveStart =
                    reservation.StartTime > openingTimeUtc
                        ? reservation.StartTime
                        : openingTimeUtc;

                var effectiveEnd =
                    reservation.EndTime < closingTimeUtc
                        ? reservation.EndTime
                        : closingTimeUtc;

                return (effectiveEnd - effectiveStart).TotalMinutes;
            });

            var status = reservedMinutes >= operatingMinutes
                ? DailyAvailabilityStatus.Booked
                : DailyAvailabilityStatus.Partial;

            results.Add(new DailyAvailability(date, status));
        }

        return results;
    }

    /// <summary>
    /// Converts a date and local time at a resource's location into UTC
    /// so it can be compared safely with PostgreSQL timestamps.
    /// </summary>
    private static DateTimeOffset ConvertToUtc(
        DateOnly date,
        TimeOnly time,
        TimeZoneInfo timeZone)
    {
        var localDateTime = date.ToDateTime(
            time,
            DateTimeKind.Unspecified);

        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(
            localDateTime,
            timeZone);

        return new DateTimeOffset(utcDateTime);
    }
}