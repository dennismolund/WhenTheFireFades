using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WhenTheFireFades.Data.Models;

public partial class TeamProposalVote
{
    [Key]
    public int TeamProposalVoteId { get; set; }

    public int TeamProposalId { get; set; }

    public int Seat { get; set; }

    public bool IsApproved { get; set; }

    [ForeignKey("TeamProposalId")]
    [InverseProperty("TeamProposalVotes")]
    public virtual TeamProposal TeamProposal { get; set; } = null!;
}
