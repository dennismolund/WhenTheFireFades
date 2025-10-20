using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class TeamProposalVote
{
    [Key]
    public int TeamProposalVoteId { get; set; }

    [Required]
    public int TeamProposalId { get; set; }

    [Required]
    public int Seat { get; set; }

    [Required]
    public bool IsApproved { get; set; }

    [Column(TypeName = "Datetime2")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(TeamProposalId))]
    public TeamProposal TeamProposal { get; set; } = default!;
}
