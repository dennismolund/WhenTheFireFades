using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Infrastructure.Persistence;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GamePlayer> GamePlayers => Set<GamePlayer>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<MissionVote> MissionVotes => Set<MissionVote>();
    public DbSet<TeamProposal> TeamProposals => Set<TeamProposal>();
    public DbSet<TeamProposalMember> TeamProposalMembers => Set<TeamProposalMember>();
    public DbSet<TeamProposalVote> TeamProposalVotes => Set<TeamProposalVote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

    }
}
