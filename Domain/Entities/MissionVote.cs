using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class MissionVote
{
    [Key]
    public int MissionVoteId { get; set; }

    [Required]
    public int RoundId { get; set; }

    [Required]
    public int Seat { get; set; }

    [Required]
    public bool IsSuccess { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(RoundId))]
    public Round Round { get; set; } = default!;
}
