namespace ReserveFlow.Services.Reviews;

/// <summary>
/// Contains the information submitted when a user reviews a resource.
/// </summary>
public sealed record CreateReviewRequest(
    Guid ReservationId,
    int Rating,
    string Comment);