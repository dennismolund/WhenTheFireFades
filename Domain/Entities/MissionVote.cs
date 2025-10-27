using System.ComponentModel.DataAnnotations;

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

    public Round Round { get; set; } = default!;
}
