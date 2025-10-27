namespace Domain.Services;

public static class MissionTeamSizeService
{
    public static int GetMissionTeamSize(int playerCount, int roundNumber)
    {
        var lookup = new Dictionary<int, int[]>
        {
            { 2, [2, 2, 2, 2, 2] }, // För testning
            { 3, [2, 3, 2, 3, 3] }, // För testning
            { 4, [2, 3, 2, 3, 3] }, // För testning
            { 5, [2, 3, 2, 3, 3] },
            { 6, [2, 3, 4, 3, 4] },
            { 7, [2, 3, 3, 4, 4] },
            { 8, [3, 4, 4, 5, 5] },
            { 9, [3, 4, 4, 5, 5] },
            { 10, [3, 4, 4, 5, 5] }
        };

        if (!lookup.TryGetValue(playerCount, out var sizes))
        {
            throw new InvalidOperationException($"Unsupported player count: {playerCount}");
        }

        if (roundNumber < 1 || roundNumber > sizes.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(roundNumber), roundNumber, "Round number is out of range.");
        }

        return sizes[roundNumber - 1];
    }
}