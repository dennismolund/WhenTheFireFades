using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Team
{
    [Key]
    public int TeamId { get; set; }

    [Required]
    public int RoundId { get; set; }

    // [Required]
    // public int AttemptNumber { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    public bool? IsApproved { get; set; }  

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(RoundId))]
    public Round Round { get; set; } = default!;

    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<TeamVote> Votes { get; set; } = new List<TeamVote>();
}
