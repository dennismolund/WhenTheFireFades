using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WhenTheFireFades.Data.Models;

public partial class Round
{
    [Key]
    public int RoundId { get; set; }

    public int GameId { get; set; }

    public int RoundNumber { get; set; }

    public int LeaderSeat { get; set; }

    [StringLength(50)]
    public string Phase { get; set; } = null!;

    [StringLength(50)]
    public string Result { get; set; } = null!;

    public int TeamSize { get; set; }

    public int SabotageCounter { get; set; }

    public int TeamVoteCounter { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("GameId")]
    [InverseProperty("Rounds")]
    public virtual Game Game { get; set; } = null!;

    [InverseProperty("Round")]
    public virtual ICollection<MissionVote> MissionVotes { get; set; } = new List<MissionVote>();

    [InverseProperty("Round")]
    public virtual ICollection<TeamProposal> TeamProposals { get; set; } = new List<TeamProposal>();
}
