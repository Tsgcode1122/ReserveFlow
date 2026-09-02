namespace ReserveFlow.Authorization;

public static class AppRoles
{
    public const string StandardUser = "StandardUser";
    public const string ResourceManager = "ResourceManager";
    public const string Administrator = "Administrator";

    // Provides one reusable collection for role seeding.
    public static readonly string[] All =
    [
        StandardUser,
        ResourceManager,
        Administrator
    ];
    // ASP.NET Core accepts comma-separated roles as an OR condition.
    public const string ManagerOrAdministrator =
        ResourceManager + "," + Administrator;
}