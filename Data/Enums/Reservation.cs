using System.ComponentModel.DataAnnotations;
using ReserveFlow.Data.Enums;

namespace ReserveFlow.Data.Entities;

public class Reservation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(150)]



    public string Purpose { get; set; } = string.Empty;

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    [Range(1, 1000)]
    public int AttendeeCount { get; set; } = 1;

    public ReservationStatus Status { get; set; }

    // Foreign key for the resource being reserved.
    public Guid ResourceId { get; set; }

    // Provides access to the complete related resource record.
    public Resource Resource { get; set; } = null!;

    // Identity uses strings as user IDs by default.
    public string UserId { get; set; } = string.Empty;

    // Provides access to the user who created the reservation.
    public ApplicationUser User { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}