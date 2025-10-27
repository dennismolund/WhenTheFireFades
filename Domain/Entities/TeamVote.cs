using System.ComponentModel.DataAnnotations;

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

    public Team Team { get; set; } = default!;
}
