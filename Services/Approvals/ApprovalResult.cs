using ReserveFlow.Data.Entities;

namespace ReserveFlow.Services.Approvals;

/// <summary>
/// Describes the result of a manager's approval decision.
/// </summary>
public sealed record ApprovalResult(
    bool Succeeded,
    string Message,
    Reservation? Reservation = null);