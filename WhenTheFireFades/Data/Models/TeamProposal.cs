using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WhenTheFireFades.Data.Models;

public partial class TeamProposal
{
    [Key]
    public int TeamProposalId { get; set; }

    public int RoundId { get; set; }

    public int AttemptNumber { get; set; }

    public bool IsActive { get; set; }

    public bool? IsApproved { get; set; }

    [ForeignKey("RoundId")]
    [InverseProperty("TeamProposals")]
    public virtual Round Round { get; set; } = null!;

    [InverseProperty("TeamProposal")]
    public virtual ICollection<TeamProposalMember> TeamProposalMembers { get; set; } = new List<TeamProposalMember>();

    [InverseProperty("TeamProposal")]
    public virtual ICollection<TeamProposalVote> TeamProposalVotes { get; set; } = new List<TeamProposalVote>();
}
