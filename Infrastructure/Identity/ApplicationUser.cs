using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
}