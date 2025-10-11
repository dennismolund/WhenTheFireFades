using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace WhenTheFireFades.Domain.Helpers;

public class SessionHelper(IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private const string TempUserIdKey = "TempUserId";
    private const string PlayerNicknameKey = "PlayerNickname";
    private const string CurrentGameCodeKey = "CurrentGameCode";

    private ISession? Session => _httpContextAccessor.HttpContext?.Session;

    public int GetOrCreateTempUserId()
    {
        var tempUserId = GetTempUserId();
        if (tempUserId == null)
        {
            tempUserId = GenerateTempUserId();
            SetTempUserId(tempUserId.Value);
            var playerNickname = $"Player#{tempUserId}";
            SetPlayerNickname(playerNickname);
        }
        return tempUserId.Value;
    }

    public int? GetTempUserId()
    {
        return Session?.GetInt32(TempUserIdKey);
    }

    public void SetTempUserId(int tempUserId)
    {
        Session?.SetInt32(TempUserIdKey, tempUserId);
    }

    //public void ClearTempUserId()
    //{
    //    Session?.Remove(TempUserIdKey);
    //}

    // Nickname Management
    public string? GetPlayerNickname()
    {
        return Session?.GetString(PlayerNicknameKey);
    }

    public void SetPlayerNickname(string nickname)
    {
        Session?.SetString(PlayerNicknameKey, nickname);
    }

    //public void ClearPlayerNickname()
    //{
    //    Session?.Remove(PlayerNicknameKey);
    //}

    public string? GetCurrentGameCode()
    {
        return Session?.GetString(CurrentGameCodeKey);
    }

    public void SetCurrentGameCode(string code)
    {
        Session?.SetString(CurrentGameCodeKey, code);
    }

    //public void ClearCurrentGameCode()
    //{
    //    Session?.Remove(CurrentGameCodeKey);
    //}

    //// Clear all session data
    //public void ClearAllGameData()
    //{
    //    ClearTempUserId();
    //    ClearPlayerNickname();
    //    ClearCurrentGameCode();
    //}

    //// Helper method to store complex objects (if needed)
    //public void SetObject<T>(string key, T value)
    //{
    //    Session?.SetString(key, JsonSerializer.Serialize(value));
    //}

    //public T? GetObject<T>(string key)
    //{
    //    var value = Session?.GetString(key);
    //    return value == null ? default : JsonSerializer.Deserialize<T>(value);
    //}

    private static int GenerateTempUserId()
    {
        return Random.Shared.Next(10000, 99999);
    }
}
