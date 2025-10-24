using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class TeamVote
{
    [Key]
    public int TeamVoteId { get; set; }

    [Required]
    public int TeamId { get; set; }

    [Required]
    public int Seat { get; set; }

    [Required]
    public bool IsApproved { get; set; }

    [Column(TypeName = "Datetime2")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(TeamId))]
    public Team Team { get; set; } = default!;
}
