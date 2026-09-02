using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReserveFlow.Authorization;
using ReserveFlow.Data;
using ReserveFlow.Data.Entities;
using ReserveFlow.Data.Enums;

namespace ReserveFlow.Services.Approvals;

public sealed class ReservationApprovalService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    UserManager<ApplicationUser> userManager)
    : IReservationApprovalService
{
    private readonly IDbContextFactory<ApplicationDbContext>
        _dbContextFactory = dbContextFactory;

    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public Task<ApprovalResult> ApproveAsync(
        Guid reservationId,
        string managerUserId,
        string? note = null,
        CancellationToken cancellationToken = default)
    {
        return ReviewAsync(
            reservationId,
            managerUserId,
            ReservationStatus.Confirmed,
            note,
            cancellationToken);
    }

    public async Task<ApprovalResult> RejectAsync(
        Guid reservationId,
        string managerUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return new ApprovalResult(
                false,
                "A rejection reason is required.");
        }

        return await ReviewAsync(
            reservationId,
            managerUserId,
            ReservationStatus.Rejected,
            reason,
            cancellationToken);
    }

    /// <summary>
    /// Verifies the manager's role and changes a pending reservation
    /// to either Confirmed or Rejected while recording an audit trail.
    /// </summary>
    private async Task<ApprovalResult> ReviewAsync(
        Guid reservationId,
        string managerUserId,
        ReservationStatus decision,
        string? note,
        CancellationToken cancellationToken)
    {
        var manager = await _userManager.FindByIdAsync(managerUserId);

        if (manager is null)
        {
            return new ApprovalResult(
                false,
                "The manager account could not be found.");
        }

        // Authorization is checked inside the service as well as the UI.
        // This prevents unauthorized calls from bypassing the page.
        var isManager = await _userManager.IsInRoleAsync(
            manager,
            AppRoles.ResourceManager);

        var isAdministrator = await _userManager.IsInRoleAsync(
            manager,
            AppRoles.Administrator);

        if (!isManager && !isAdministrator)
        {
            return new ApprovalResult(
                false,
                "You are not authorized to review reservations.");
        }

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var reservation = await dbContext.Reservations
            .SingleOrDefaultAsync(
                reservation => reservation.Id == reservationId,
                cancellationToken);

        if (reservation is null)
        {
            return new ApprovalResult(
                false,
                "The reservation could not be found.");
        }

        if (reservation.Status != ReservationStatus.Pending)
        {
            return new ApprovalResult(
                false,
                "This reservation has already been reviewed or cancelled.");
        }

        reservation.Status = decision;
        reservation.ReviewedById = managerUserId;
        reservation.ReviewedAt = DateTimeOffset.UtcNow;
        reservation.ApprovalNote = string.IsNullOrWhiteSpace(note)
            ? null
            : note.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);

        var message = decision == ReservationStatus.Confirmed
            ? "The reservation was approved."
            : "The reservation was rejected.";

        return new ApprovalResult(true, message, reservation);
    }
}