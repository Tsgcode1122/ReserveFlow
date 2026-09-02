namespace ReserveFlow.Services.Reservations;

public interface IReservationService
{
    /// <summary>
    /// Validates and creates a reservation for the specified user.
    /// </summary>
    Task<ReservationResult> CreateAsync(
        CreateReservationRequest request,
        string userId,
        CancellationToken cancellationToken = default);
    /// <summary>
    /// Cancels an eligible reservation belonging to the specified user.
    /// </summary>
    Task<ReservationResult> CancelAsync(
        Guid reservationId,
        string userId,
        CancellationToken cancellationToken = default);
}