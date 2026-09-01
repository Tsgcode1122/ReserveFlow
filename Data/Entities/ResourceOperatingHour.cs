namespace ReserveFlow.Data.Entities;

public class ResourceOperatingHour
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Identifies the resource whose schedule is being configured.
    public Guid ResourceId { get; set; }

    public Resource Resource { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly OpenTime { get; set; }

    public TimeOnly CloseTime { get; set; }

    // Allows a resource to be unavailable for an entire weekday.
    public bool IsClosed { get; set; }
}