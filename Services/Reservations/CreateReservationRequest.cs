namespace ReserveFlow.Services.Reservations;

/// <summary>
/// Contains the information needed to create a reservation.
/// </summary>
public sealed record CreateReservationRequest(
    Guid ResourceId,
    string Title,
    string? Purpose,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int AttendeeCount);