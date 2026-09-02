using System.ComponentModel.DataAnnotations;
using ReserveFlow.Data.Enums;

namespace ReserveFlow.Data.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Identifies the user who should receive the notification.
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    // A notification may refer to a reservation.
    public Guid? ReservationId { get; set; }

    public Reservation? Reservation { get; set; }

    public NotificationType Type { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ReadAt { get; set; }
}