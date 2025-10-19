using WhenTheFireFades.Models;

namespace WhenTheFireFades.ViewModels;

public sealed class PlayViewModel
{
    public Game Game { get; set; } = default!;
    public GamePlayer CurrentPlayer { get; set; } = default!;
    public Round CurrentRound { get; set; } = default!;
    public GamePlayer CurrentLeader { get; set; } = default!;

    public TeamProposal? ActiveTeamProposal { get; set; }
    public List<GamePlayer>? ProposedTeamMembers { get; set; }
    public List<TeamProposalVote>? TeamProposalVotes { get; set; }
    public bool HasCurrentPlayerVoted { get; set; }
    public List<MissionVote>? MissionVotes { get; set; }

    public bool IsLeader => CurrentPlayer.Seat == CurrentLeader.Seat;
    public bool IsTeamSelectionPhase => CurrentRound.Status == RoundStatus.TeamSelection;
    public bool IsVotingPhase => CurrentRound.Status == RoundStatus.VoteOnTeam;
    public bool IsMissionPhase => CurrentRound.Status == RoundStatus.SecretChoices;

    public int CurrentAttemptNumber => ActiveTeamProposal?.AttemptNumber ?? Game.ConsecutiveRejectedProposals + 1;
    public int RemainingAttempts => 5 - Game.ConsecutiveRejectedProposals;

    public bool IsOnMissionTeam
    {
        get
        {
            if (ProposedTeamMembers == null || !IsMissionPhase) return false;

            return ProposedTeamMembers.Any(p => p.Seat == CurrentPlayer.Seat);
        }
    }

}
