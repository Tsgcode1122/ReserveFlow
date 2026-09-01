using ReserveFlow.Data.Entities;

namespace ReserveFlow.Services.Reviews;

/// <summary>
/// Describes whether a review was accepted or rejected.
/// </summary>
public sealed record ReviewResult(
    bool Succeeded,
    string Message,
    ResourceReview? Review = null);