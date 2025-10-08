using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using WhenTheFireFades.Data.Models;

namespace WhenTheFireFades.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    //public DbSet<Game> Games { get; set; }

    //public DbSet<GamePlayer> GamePlayers { get; set; }

    //public DbSet<MissionVote> MissionVotes { get; set; }

    //public DbSet<Round> Rounds { get; set; }

    //public DbSet<TeamProposal> TeamProposals { get; set; }

    //public DbSet<TeamProposalMember> TeamProposalMembers { get; set; }

    //public DbSet<TeamProposalVote> TeamProposalVotes { get; set; }

}
