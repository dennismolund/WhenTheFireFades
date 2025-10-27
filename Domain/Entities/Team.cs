using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Team
{
    [Key]
    public int TeamId { get; set; }

    [Required]
    public int RoundId { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    public bool? IsApproved { get; set; }  

    public Round Round { get; set; } = default!;

    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<TeamVote> Votes { get; set; } = new List<TeamVote>();
}
