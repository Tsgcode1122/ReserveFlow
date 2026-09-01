namespace ReserveFlow.Services.Reservations;

/// <summary>
/// Contains the information required to reserve a resource.
/// </summary>
public sealed record CreateReservationRequest(
    Guid ResourceId,
    string Purpose,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int AttendeeCount);