using System.ComponentModel.DataAnnotations;

namespace ReserveFlow.Data.Entities;

public class Location
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? Building { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();

    [Required]
    [MaxLength(100)]
    public string TimeZoneId { get; set; } = "America/New_York";
}