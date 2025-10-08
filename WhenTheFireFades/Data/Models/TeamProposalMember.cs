using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WhenTheFireFades.Data.Models;

public partial class TeamProposalMember
{
    [Key]
    public int TeamProposalMemberId { get; set; }

    public int TeamProposalId { get; set; }

    public int Seat { get; set; }

    [ForeignKey("TeamProposalId")]
    [InverseProperty("TeamProposalMembers")]
    public virtual TeamProposal TeamProposal { get; set; } = null!;
}
