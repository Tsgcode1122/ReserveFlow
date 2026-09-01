using System.ComponentModel.DataAnnotations;

namespace ReserveFlow.Data.Entities;

public class ResourceReview
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Identifies the resource being reviewed.
    public Guid ResourceId { get; set; }

    public Resource Resource { get; set; } = null!;

    // Identifies the user who submitted the review.
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    // Connects the review to the completed reservation that made
    // the user eligible to submit it.
    public Guid ReservationId { get; set; }

    public Reservation Reservation { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}