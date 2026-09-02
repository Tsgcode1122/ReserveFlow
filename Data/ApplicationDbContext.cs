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

        // Connects each policy notice to its resource. These notices are
        // informational and are not automatically enforced during booking.
        builder.Entity<ResourcePolicyNotice>()
            .HasOne(notice => notice.Resource)
            .WithMany(resource => resource.PolicyNotices)
            .HasForeignKey(notice => notice.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ensures that each reservation can produce only one review.
        builder.Entity<ResourceReview>()
            .HasIndex(review => review.ReservationId)
            .IsUnique();

        // Connects reviews to the resource being reviewed.
        builder.Entity<ResourceReview>()
            .HasOne(review => review.Resource)
            .WithMany(resource => resource.Reviews)
            .HasForeignKey(review => review.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Connects reviews to the user who submitted them.
        builder.Entity<ResourceReview>()
            .HasOne(review => review.User)
            .WithMany(user => user.Reviews)
            .HasForeignKey(review => review.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Creates the one-to-one relationship between a reservation
        // and the review produced from it.
        builder.Entity<ResourceReview>()
            .HasOne(review => review.Reservation)
            .WithOne(reservation => reservation.Review)
            .HasForeignKey<ResourceReview>(review => review.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);
        // Connects each operating-hours record to its resource.
        builder.Entity<ResourceOperatingHour>()
            .HasOne(schedule => schedule.Resource)
            .WithMany(resource => resource.OperatingHours)
            .HasForeignKey(schedule => schedule.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Prevents a resource from having duplicate schedules for one weekday.
        builder.Entity<ResourceOperatingHour>()
            .HasIndex(schedule => new
            {
                schedule.ResourceId,
                schedule.DayOfWeek
            })
            .IsUnique();

        // Connects a reservation decision to the manager who made it.
        // The relationship is optional because new reservations have not
        // yet been reviewed.
        builder.Entity<Reservation>()
            .HasOne(reservation => reservation.ReviewedBy)
            .WithMany(user => user.ReviewedReservations)
            .HasForeignKey(reservation => reservation.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
    // Represents all resource booking records.
    public DbSet<Reservation> Reservations => Set<Reservation>();
    // Represents informational policy notices shown to users.
    public DbSet<ResourcePolicyNotice> ResourcePolicyNotices =>
        Set<ResourcePolicyNotice>();
    // Represents ratings and comments submitted for resources.
    public DbSet<ResourceReview> ResourceReviews =>
        Set<ResourceReview>();
    // Represents the weekly operating schedules for resources.
    public DbSet<ResourceOperatingHour> ResourceOperatingHours =>
        Set<ResourceOperatingHour>();
}