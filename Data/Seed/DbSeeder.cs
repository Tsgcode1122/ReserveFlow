using Microsoft.EntityFrameworkCore;
using ReserveFlow.Data.Entities;
using ReserveFlow.Data.Enums;

namespace ReserveFlow.Data.Seed;

public static class DbSeeder
{
    /// <summary>
    /// Adds development data when the Resources table is empty.
    /// Running the application repeatedly will not duplicate the data.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        // Creates a temporary dependency-injection scope so that the
        // scoped ApplicationDbContext can be used during startup.
        using var scope = services.CreateScope();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Stop if the sample resources were previously inserted.
        if (await dbContext.Resources.AnyAsync())
        {
            return;
        }

        var buildingA = new Location
        {
            Name = "Building A",
            Building = "Building A",
            Address = "Main Campus"
        };

        var researchCenter = new Location
        {
            Name = "Research Center",
            Building = "Research Center",
            Address = "Research Campus"
        };

        var towerC = new Location
        {
            Name = "Tower C",
            Building = "Tower C",
            Address = "Main Campus"
        };

        var pavilionD = new Location
        {
            Name = "Pavilion D",
            Building = "Pavilion D",
            Address = "Technology Campus"
        };

        var facilitiesGarage = new Location
        {
            Name = "Facilities Garage",
            Building = "Facilities Garage",
            Address = "Operations Campus"
        };

        var wifi = new Amenity
        {
            Name = "Wi-Fi",
            Description = "Wireless internet access"
        };

        var videoConferencing = new Amenity
        {
            Name = "Video Conferencing",
            Description = "Camera and conferencing equipment"
        };

        var whiteboard = new Amenity
        {
            Name = "Whiteboard",
            Description = "Wall-mounted writing surface"
        };

        var displayScreen = new Amenity
        {
            Name = "Display Screen",
            Description = "Presentation display"
        };

        var climateControl = new Amenity
        {
            Name = "Climate Control",
            Description = "Adjustable room temperature"
        };

        var executiveBoardroom = new Resource
        {
            Name = "Executive Boardroom",
            Description = "Premium boardroom for presentations and meetings.",
            Type = ResourceType.MeetingRoom,
            Capacity = 20,
            ApprovalMode = ApprovalMode.Instant,
            Location = buildingA,
            Amenities =
            [
                wifi,
                videoConferencing,
                whiteboard,
                displayScreen,
                climateControl
            ]
        };

        var innovationHub = new Resource
        {
            Name = "Innovation Hub — Room 3A",
            Description = "Collaborative space for workshops and planning.",
            Type = ResourceType.MeetingRoom,
            Capacity = 12,
            ApprovalMode = ApprovalMode.Required,
            Location = researchCenter,
            Amenities =
            [
                wifi,
                videoConferencing,
                whiteboard
            ]
        };

        var focusPod = new Resource
        {
            Name = "Focus Pod Suite 7",
            Description = "Private workspace for focused individual work.",
            Type = ResourceType.Workspace,
            Capacity = 4,
            ApprovalMode = ApprovalMode.Instant,
            Location = towerC,
            Amenities =
            [
                wifi,
                displayScreen
            ]
        };

        var projector = new Resource
        {
            Name = "Sony 4K Projector Kit",
            Description = "Portable projector kit for presentations.",
            Type = ResourceType.Equipment,
            Capacity = 1,
            ApprovalMode = ApprovalMode.Required,
            Location = pavilionD
        };

        var fleetVan = new Resource
        {
            Name = "Fleet Van — MV-007",
            Description = "Passenger vehicle for organizational travel.",
            Type = ResourceType.Vehicle,
            Capacity = 7,
            ApprovalMode = ApprovalMode.Required,
            Location = facilitiesGarage,
            Amenities =
            [
                climateControl
            ]
        };

        // Adding the resources also adds their related locations and
        // amenities because EF Core tracks the complete object graph.
        dbContext.Resources.AddRange(
            executiveBoardroom,
            innovationHub,
            focusPod,
            projector,
            fleetVan);

        await dbContext.SaveChangesAsync();
    }
}