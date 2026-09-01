using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReserveFlow.Data.Entities;

namespace ReserveFlow.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Location> Locations => Set<Location>();

    public DbSet<Resource> Resources => Set<Resource>();
}