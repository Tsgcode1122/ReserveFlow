using Microsoft.EntityFrameworkCore;
using ReserveFlow.Data;
using ReserveFlow.Data.Entities;
using ReserveFlow.Data.Enums;

namespace ReserveFlow.Services.Reservations;

public sealed class ReservationService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory)
    : IReservationService
{
    private readonly IDbContextFactory<ApplicationDbContext>
        _dbContextFactory = dbContextFactory;
    /// <summary>
    /// Validates the booking request, checks for scheduling conflicts,
    /// determines its initial status, and saves it to PostgreSQL.
    /// </summary>
    public async Task<ReservationResult> CreateAsync(
        CreateReservationRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Purpose))
        {
            return new ReservationResult(
                false,
                "Please provide the purpose of this reservation.");
        }

        if (request.StartTime < DateTimeOffset.UtcNow)
        {
            return new ReservationResult(
                false,
                "A reservation cannot start in the past.");
        }

        if (request.EndTime <= request.StartTime)
        {
            return new ReservationResult(
                false,
                "The end time must be later than the start time.");
        }
        // A fresh context is created for this individual operation.
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        // Loads only an active resource matching the selected ID.
        var resource = await dbContext.Resources
            .SingleOrDefaultAsync(
                resource =>
                    resource.Id == request.ResourceId &&
                    resource.IsActive,
                cancellationToken);

        if (resource is null)
        {
            return new ReservationResult(
                false,
                "The selected resource does not exist or is inactive.");
        }

        if (request.AttendeeCount < 1)
        {
            return new ReservationResult(
                false,
                "At least one attendee is required.");
        }

        if (request.AttendeeCount > resource.Capacity)
        {
            return new ReservationResult(
                false,
                $"This resource can accommodate only {resource.Capacity} people.");
        }

        // Pending and confirmed reservations hold the selected time slot.
        var blockingStatuses = new[]
        {
            ReservationStatus.Pending,
            ReservationStatus.Confirmed,
            ReservationStatus.CheckedIn
        };

        // Two time ranges overlap when the existing reservation starts
        // before the requested end and ends after the requested start.
        var hasConflict = await dbContext.Reservations.AnyAsync(
            reservation =>
                reservation.ResourceId == request.ResourceId &&
                blockingStatuses.Contains(reservation.Status) &&
                reservation.StartTime < request.EndTime &&
                request.StartTime < reservation.EndTime,
            cancellationToken);

        if (hasConflict)
        {
            return new ReservationResult(
                false,
                "The resource is already reserved during this period.");
        }

        // Approval-based resources begin as Pending. Instant-booking
        // resources are confirmed immediately.
        var initialStatus = resource.ApprovalMode == ApprovalMode.Required
            ? ReservationStatus.Pending
            : ReservationStatus.Confirmed;

        var reservation = new Reservation
        {
            ResourceId = request.ResourceId,
            UserId = userId,

            Purpose = request.Purpose.Trim(),
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            AttendeeCount = request.AttendeeCount,
            Status = initialStatus
        };

        dbContext.Reservations.Add(reservation);
        await dbContext.SaveChangesAsync(cancellationToken);

        var message = initialStatus == ReservationStatus.Pending
            ? "Your reservation was submitted for approval."
            : "Your reservation was confirmed.";

        return new ReservationResult(true, message, reservation);
    }
    /// <summary>
    /// Cancels a future pending or confirmed reservation after verifying
    /// that it belongs to the signed-in user.
    /// </summary>
    public async Task<ReservationResult> CancelAsync(
        Guid reservationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var reservation = await dbContext.Reservations
            .SingleOrDefaultAsync(
                reservation => reservation.Id == reservationId,
                cancellationToken);

        if (reservation is null)
        {
            return new ReservationResult(
                false,
                "The reservation could not be found.");
        }

        if (reservation.UserId != userId)
        {
            return new ReservationResult(
                false,
                "You cannot cancel another user's reservation.");
        }

        var cancellableStatuses = new[]
        {
        ReservationStatus.Pending,
        ReservationStatus.Confirmed
    };

        if (!cancellableStatuses.Contains(reservation.Status))
        {
            return new ReservationResult(
                false,
                "This reservation can no longer be cancelled.");
        }

        if (reservation.StartTime <= DateTimeOffset.UtcNow)
        {
            return new ReservationResult(
                false,
                "A reservation cannot be cancelled after it has started.");
        }

        reservation.Status = ReservationStatus.Cancelled;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ReservationResult(
            true,
            "The reservation was cancelled.",
            reservation);
    }
}
