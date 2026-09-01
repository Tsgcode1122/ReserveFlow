using Microsoft.AspNetCore.Identity;
using ReserveFlow.Data.Entities;
namespace ReserveFlow.Data;

// Add profile data for application users by adding properties to the ApplicationUser class


public class ApplicationUser : IdentityUser
{
    // Contains every reservation created by this user.
    public ICollection<Reservation> Reservations { get; set; }
        = new List<Reservation>();
    // Contains reviews submitted by this user.
    public ICollection<ResourceReview> Reviews { get; set; }
        = new List<ResourceReview>();
}