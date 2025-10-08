using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WhenTheFireFades.Data.Models;

[Table("MissionVote")]
public partial class MissionVote
{
    [Key]
    public int MissionVoteId { get; set; }

    public int RoundId { get; set; }

    public int Seat { get; set; }

    public bool IsSabotage { get; set; }

    [ForeignKey("RoundId")]
    [InverseProperty("MissionVotes")]
    public virtual Round Round { get; set; } = null!;
}
