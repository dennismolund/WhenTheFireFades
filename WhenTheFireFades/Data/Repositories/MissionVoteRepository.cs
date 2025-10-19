using Microsoft.EntityFrameworkCore;
using WhenTheFireFades.Models;

namespace WhenTheFireFades.Data.Repositories;

public class MissionVoteRepository (ApplicationDbContext db) : IMissionVoteRepository 
{
    private readonly ApplicationDbContext _db = db;

    public async Task AddMissionVoteAsync(MissionVote missionVote)
    {
        await _db.AddAsync(missionVote);
    }

    public async Task<List<MissionVote>> GetByRoundIdAsync(int roundId)
    {
        return await _db.MissionVotes
            .Where(v => v.RoundId == roundId)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
