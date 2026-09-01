using System.ComponentModel.DataAnnotations;
using ReserveFlow.Data.Enums;

namespace ReserveFlow.Data.Entities;

public class Resource
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public ResourceType Type { get; set; }

    [Range(1, 1000)]
    public int Capacity { get; set; } = 1;

    public ApprovalMode ApprovalMode { get; set; } = ApprovalMode.Instant;

    public Guid LocationId { get; set; }

    public Location Location { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    // Contains the current and historical reservations for this resource.
    public ICollection<Reservation> Reservations { get; set; }
        = new List<Reservation>();
}