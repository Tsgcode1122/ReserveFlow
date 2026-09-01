using Microsoft.EntityFrameworkCore;
using ReserveFlow.Data;
using ReserveFlow.Data.Entities;
using ReserveFlow.Data.Enums;

namespace ReserveFlow.Services.Reviews;

public sealed class ReviewService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory)
    : IReviewService
{
    private readonly IDbContextFactory<ApplicationDbContext>
        _dbContextFactory = dbContextFactory;

    /// <summary>
    /// Confirms that the user completed the reservation and has not
    /// previously reviewed it before saving the review.
    /// </summary>
    public async Task<ReviewResult> CreateAsync(
        CreateReviewRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (request.Rating is < 1 or > 5)
        {
            return new ReviewResult(
                false,
                "Please select a rating between 1 and 5.");
        }

        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            return new ReviewResult(
                false,
                "Please enter a review comment.");
        }

        if (request.Comment.Trim().Length > 1000)
        {
            return new ReviewResult(
                false,
                "The review cannot exceed 1,000 characters.");
        }

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var reservation = await dbContext.Reservations
            .AsNoTracking()
            .SingleOrDefaultAsync(
                reservation =>
                    reservation.Id == request.ReservationId,
                cancellationToken);

        if (reservation is null)
        {
            return new ReviewResult(
                false,
                "The associated reservation could not be found.");
        }

        // Prevents a user from reviewing another person's reservation.
        if (reservation.UserId != userId)
        {
            return new ReviewResult(
                false,
                "You cannot review another user's reservation.");
        }

        if (reservation.Status != ReservationStatus.Completed)
        {
            return new ReviewResult(
                false,
                "You can review this resource after completing the reservation.");
        }

        var alreadyReviewed = await dbContext.ResourceReviews
            .AnyAsync(
                review =>
                    review.ReservationId == request.ReservationId,
                cancellationToken);

        if (alreadyReviewed)
        {
            return new ReviewResult(
                false,
                "A review has already been submitted for this reservation.");
        }

        var review = new ResourceReview
        {
            ReservationId = reservation.Id,
            ResourceId = reservation.ResourceId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment.Trim()
        };

        dbContext.ResourceReviews.Add(review);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ReviewResult(
            true,
            "Thank you. Your review was submitted.",
            review);
    }
}