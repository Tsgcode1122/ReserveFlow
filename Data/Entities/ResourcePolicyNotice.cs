using System.ComponentModel.DataAnnotations;

namespace ReserveFlow.Data.Entities;

public class ResourcePolicyNotice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Identifies the resource to which this notice belongs.
    public Guid ResourceId { get; set; }

    public Resource Resource { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    // Controls the order in which notices appear in the Policies tab.
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}