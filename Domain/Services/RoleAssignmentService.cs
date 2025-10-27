using Domain.Entities;
using Domain.Enums;
using Domain.Rules;

namespace Domain.Services;

public static class RoleAssignmentService
{
    private static readonly Random Random = new();
    
    public static void AssignRoles(List<GamePlayer> players)
    {
        var shuffled = players
            .OrderBy(_ => Random.Next())
            .ToList();

        foreach (var player in shuffled)
        {
            player.Role = PlayerRole.Human;
        }

        var shapeshifterCount = GameRules.GetShapeshifterCount(players.Count);
        for (var i = 0; i < shapeshifterCount; i++)
        {
            shuffled[i].Role = PlayerRole.Shapeshifter;
        }
    }
    
    
}