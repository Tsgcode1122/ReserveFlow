using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReserveFlow.Data.Entities;

namespace ReserveFlow.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    // Represents the Locations table in PostgreSQL.
    public DbSet<Location> Locations => Set<Location>();

    // Represents all reservable rooms, workspaces, equipment, and vehicles.
    public DbSet<Resource> Resources => Set<Resource>();

    // Represents features that can be assigned to resources, such as Wi-Fi.
    public DbSet<Amenity> Amenities => Set<Amenity>();

    /// <summary>
    /// Configures database constraints and relationships when EF Core
    /// creates the model for this application.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Preserves the ASP.NET Core Identity table configuration.
        base.OnModelCreating(builder);

        // Prevents multiple amenities from having the same name.
        builder.Entity<Amenity>()
            .HasIndex(amenity => amenity.Name)
            .IsUnique();

        // Configures the many-to-many relationship between resources
        // and amenities through the ResourceAmenities joining table.
        builder.Entity<Resource>()
            .HasMany(resource => resource.Amenities)
            .WithMany(amenity => amenity.Resources)
            .UsingEntity("ResourceAmenities");

        // Connects each reservation to one resource while preventing a resource
        // with booking history from being accidentally deleted.
        builder.Entity<Reservation>()
            .HasOne(reservation => reservation.Resource)
            .WithMany(resource => resource.Reservations)
            .HasForeignKey(reservation => reservation.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Connects each reservation to the Identity user who created it.
        builder.Entity<Reservation>()
            .HasOne(reservation => reservation.User)
            .WithMany(user => user.Reservations)
            .HasForeignKey(reservation => reservation.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    // Represents all resource booking records.
    public DbSet<Reservation> Reservations => Set<Reservation>();
}