using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class TeamMember
{
    [Key]
    public int TeamMemberId { get; set; }

    [Required]
    public int TeamId { get; set; }

    [Required]
    public int Seat { get; set; }

    public Team Team { get; set; } = default!;
}
