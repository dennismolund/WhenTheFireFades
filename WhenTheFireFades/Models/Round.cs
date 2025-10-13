using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WhenTheFireFades.Models;

[Index(nameof(GameId), nameof(RoundNumber), IsUnique = true)] // one RoundNumber per game
public class Round
{
    [Key]
    public int RoundId { get; set; }

    [Required]
    public int GameId { get; set; }

    [Required]
    public int RoundNumber { get; set; } // 1..5

    [Required]
    public int LeaderSeat { get; set; }

    [Required]
    public RoundStatus Status { get; set; } = RoundStatus.TeamSelection;

    public RoundResult? Result { get; set; } = RoundResult.Unknown;

    [Required]
    public int TeamSize { get; set; }

    [Required]
    public int SabotageCounter { get; set; } = 0;

    [Required]
    public int TeamVoteCounter { get; set; } = 0;

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(GameId))]
    public Game Game { get; set; } = default!;

    public ICollection<TeamProposal> TeamProposals { get; set; } = new List<TeamProposal>();
    public ICollection<MissionVote> MissionVotes { get; set; } = new List<MissionVote>();
}
