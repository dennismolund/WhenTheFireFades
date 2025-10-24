using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class TeamMember
{
    [Key]
    public int TeamMemberId { get; set; }

    [Required]
    public int TeamId { get; set; }

    [Required]
    public int Seat { get; set; }

    [ForeignKey(nameof(TeamId))]
    public Team Team { get; set; } = default!;
}
