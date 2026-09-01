namespace ReserveFlow.Services.Availability;

/// <summary>
/// Represents one date and its calculated availability.
/// </summary>
public sealed record DailyAvailability(
    DateOnly Date,
    DailyAvailabilityStatus Status);