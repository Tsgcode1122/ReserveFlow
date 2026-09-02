namespace ReserveFlow.Services.Approvals;

public interface IReservationApprovalService
{
    Task<ApprovalResult> ApproveAsync(
        Guid reservationId,
        string managerUserId,
        string? note = null,
        CancellationToken cancellationToken = default);

    Task<ApprovalResult> RejectAsync(
        Guid reservationId,
        string managerUserId,
        string reason,
        CancellationToken cancellationToken = default);
}