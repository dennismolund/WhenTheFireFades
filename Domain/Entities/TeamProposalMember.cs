using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class TeamProposalMember
{
    [Key]
    public int TeamProposalMemberId { get; set; }

    [Required]
    public int TeamProposalId { get; set; }

    [Required]
    public int Seat { get; set; }

    [ForeignKey(nameof(TeamProposalId))]
    public TeamProposal TeamProposal { get; set; } = default!;
}
