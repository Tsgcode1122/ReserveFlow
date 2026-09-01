using ReserveFlow.Data.Entities;

namespace ReserveFlow.Services.Reservations;

/// <summary>
/// Returns the outcome of a reservation attempt.
/// </summary>
public sealed record ReservationResult(
    bool Succeeded,
    string Message,
    Reservation? Reservation = null);