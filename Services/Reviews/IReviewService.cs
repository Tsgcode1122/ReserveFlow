namespace ReserveFlow.Services.Reviews;

public interface IReviewService
{
    /// <summary>
    /// Validates and saves a review for a completed reservation.
    /// </summary>
    Task<ReviewResult> CreateAsync(
        CreateReviewRequest request,
        string userId,
        CancellationToken cancellationToken = default);
}