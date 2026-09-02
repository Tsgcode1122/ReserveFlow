using Microsoft.EntityFrameworkCore;
using ReserveFlow.Data.Entities;
using ReserveFlow.Data.Enums;
using Microsoft.AspNetCore.Identity;
using ReserveFlow.Authorization;
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
        await SeedRolesAsync(scope.ServiceProvider);
        await SeedDevelopmentUserRolesAsync(scope.ServiceProvider);
        // Stop if the sample resources were previously inserted.
        if (await dbContext.Resources.AnyAsync())
        {
            // Seed newer development data even when the original resources
            // were added during an earlier application run.
            await SeedPolicyNoticesAsync(dbContext);
            await SeedOperatingHoursAsync(dbContext);
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
        // Ensures the application's required Identity roles exist.

        await dbContext.SaveChangesAsync();
        await SeedPolicyNoticesAsync(dbContext);
    }
    /// <summary>
    /// Adds informational policy notices to the sample resources.
    /// This method does not create duplicates when the application restarts.
    /// </summary>
    /// <summary>
    /// Creates a weekly operating schedule for each sample resource.
    /// Weekdays are open from 7:00 AM until 10:00 PM, while weekends
    /// are marked as closed.
    /// </summary>
    private static async Task SeedOperatingHoursAsync(
        ApplicationDbContext dbContext)
    {
        // Prevent duplicate weekday schedules during later application runs.
        if (await dbContext.ResourceOperatingHours.AnyAsync())
        {
            return;
        }

        var resources = await dbContext.Resources.ToListAsync();
        var schedules = new List<ResourceOperatingHour>();

        foreach (var resource in resources)
        {
            foreach (var day in Enum.GetValues<DayOfWeek>())
            {
                var isWeekend =
                    day is DayOfWeek.Saturday or DayOfWeek.Sunday;

                schedules.Add(new ResourceOperatingHour
                {
                    ResourceId = resource.Id,
                    DayOfWeek = day,

                    // Closed days still receive default time values, but
                    // availability logic will ignore them when IsClosed is true.
                    OpenTime = isWeekend
                        ? TimeOnly.MinValue
                        : new TimeOnly(7, 0),

                    CloseTime = isWeekend
                        ? TimeOnly.MinValue
                        : new TimeOnly(22, 0),

                    IsClosed = isWeekend
                });
            }
        }

        dbContext.ResourceOperatingHours.AddRange(schedules);
        await dbContext.SaveChangesAsync();
    }
    private static async Task SeedPolicyNoticesAsync(
        ApplicationDbContext dbContext)
    {
        if (await dbContext.ResourcePolicyNotices.AnyAsync())
        {
            return;
        }

        var executiveBoardroom = await dbContext.Resources
            .SingleAsync(resource =>
                resource.Name == "Executive Boardroom");

        var innovationHub = await dbContext.Resources
            .SingleAsync(resource =>
                resource.Name == "Innovation Hub — Room 3A");

        var fleetVan = await dbContext.Resources
            .SingleAsync(resource =>
                resource.Name == "Fleet Van — MV-007");

        var notices = new[]
        {
        new ResourcePolicyNotice
        {
            ResourceId = executiveBoardroom.Id,
            Message = "Bookings require department-head approval.",
            DisplayOrder = 1
        },
        new ResourcePolicyNotice
        {
            ResourceId = executiveBoardroom.Id,
            Message = "Catering must be ordered at least 48 hours in advance.",
            DisplayOrder = 2
        },
        new ResourcePolicyNotice
        {
            ResourceId = executiveBoardroom.Id,
            Message = "Setup and teardown are included in the booking time.",
            DisplayOrder = 3
        },
        new ResourcePolicyNotice
        {
            ResourceId = executiveBoardroom.Id,
            Message = "Please leave the room in its original condition.",
            DisplayOrder = 4
        },

        new ResourcePolicyNotice
        {
            ResourceId = innovationHub.Id,
            Message = "Return movable furniture to its original arrangement.",
            DisplayOrder = 1
        },
        new ResourcePolicyNotice
        {
            ResourceId = innovationHub.Id,
            Message = "Food and uncovered drinks are not permitted.",
            DisplayOrder = 2
        },

        new ResourcePolicyNotice
        {
            ResourceId = fleetVan.Id,
            Message = "Only approved organizational drivers may operate this vehicle.",
            DisplayOrder = 1
        },
        new ResourcePolicyNotice
        {
            ResourceId = fleetVan.Id,
            Message = "Return the vehicle with at least half a tank of fuel.",
            DisplayOrder = 2
        }
    };

        dbContext.ResourcePolicyNotices.AddRange(notices);
        await dbContext.SaveChangesAsync();
        await SeedPolicyNoticesAsync(dbContext);
        await SeedOperatingHoursAsync(dbContext);
    }
    /// <summary>
    /// Creates the application roles when they do not already exist.
    /// </summary>
    private static async Task SeedRolesAsync(IServiceProvider services)

    {
        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var roleName in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(roleName));

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        result.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"Could not create role '{roleName}': {errors}");
                }
            }
        }
    }

    /// <summary>
    /// Assigns StandardUser to existing accounts and optionally promotes
    /// one development account to ResourceManager using User Secrets.
    /// </summary>
    /// 

    private static async Task SeedDevelopmentUserRolesAsync(
        IServiceProvider services)
    {
        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var configuration =
            services.GetRequiredService<IConfiguration>();

        var users = await userManager.Users.ToListAsync();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);

            if (roles.Count == 0)
            {
                await userManager.AddToRoleAsync(
                    user,
                    AppRoles.StandardUser);
            }
        }

        var managerEmail =
            configuration["Development:ManagerEmail"];

        if (string.IsNullOrWhiteSpace(managerEmail))
        {
            return;
        }

        var manager = await userManager.FindByEmailAsync(managerEmail);

        if (manager is not null &&
            !await userManager.IsInRoleAsync(
                manager,
                AppRoles.ResourceManager))
        {
            await userManager.AddToRoleAsync(
                manager,
                AppRoles.ResourceManager);
        }
    }
}