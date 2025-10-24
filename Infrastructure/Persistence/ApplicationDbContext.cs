using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GamePlayer> GamePlayers => Set<GamePlayer>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<MissionVote> MissionVotes => Set<MissionVote>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<TeamVote> TeamVotes => Set<TeamVote>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<GamePlayer>()
            .Property(gp => gp.UserId)
            .HasMaxLength(450);

        builder.Entity<GamePlayer>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.GamePlayers)
            .HasForeignKey(gp => gp.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
