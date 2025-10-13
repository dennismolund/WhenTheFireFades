using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WhenTheFireFades.Data;
using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Domain.Helpers;
using WhenTheFireFades.Domain.Services;
using WhenTheFireFades.Hubs;

namespace WhenTheFireFades.Controllers;

public class GameController(
    GameService gameService, 
    IGameRepository gameRepository, 
    IGamePlayerRepository gamePlayerRepository,
    IRoundRepository roundRepository,
    SessionHelper sessionHelper, 
    IHubContext<GameLobbyHub> hubContext) : Controller
{
    private readonly GameService _gameService = gameService;
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IGamePlayerRepository _gamePlayerRepository = gamePlayerRepository;
    private readonly IRoundRepository _roundRepository = roundRepository;
    private readonly SessionHelper _sessionHelper = sessionHelper;
    private readonly IHubContext<GameLobbyHub> _hubContext = hubContext;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create()
    {
        var tempUserId = _sessionHelper.GetOrCreateTempUserId();

        var game = await _gameService.CreateGameAsync();
        var gamePlayer = await _gameService.CreateGamePlayerAsync(game, tempUserId);

        _sessionHelper.SetCurrentGameCode(game.ConnectionCode);
        return RedirectToAction(nameof(Lobby), new { code = game.ConnectionCode });
    }
    
    [HttpGet]
    public async Task<IActionResult> Lobby(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Code is required.");

        code = code.Trim().ToUpperInvariant();

        var game = await _gameRepository.GetByCodeWithPlayersAsync(code);
        if (game == null)
        {
            return NotFound();
        }

        var tempUserId = _sessionHelper.GetOrCreateTempUserId();
        var existingPlayer = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId);

        if (existingPlayer == null)
        {
            var gamePlayer = await _gameService.CreateGamePlayerAsync(game, tempUserId);

            game = await _gameRepository.GetByCodeWithPlayersAsync(code);
        }

        ViewBag.TempUserId = tempUserId;
        ViewBag.PlayerNickname = _sessionHelper.GetPlayerNickname();
        ViewBag.GameCode = code;

        return View(game);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LeaveGameLobby(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Code is required.");

        code = code.Trim().ToUpperInvariant();

        var game = await _gameRepository.GetByCodeWithPlayersAsync(code);
        if (game == null)
        {
            return NotFound();
        }

        var tempUserId = _sessionHelper.GetTempUserId();
        if (tempUserId == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var player = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId);
        if (player != null)
        {
            _gamePlayerRepository.RemovePlayer(player);
            await _gamePlayerRepository.SaveChangesAsync();

            game = await _gameRepository.GetByCodeWithPlayersAsync(code);

            await _hubContext.Clients.Group(code).SendAsync("PlayerLeft", new
            {
                leftUserId = tempUserId,
                players = game.Players.Select(p => new
                {
                    p.TempUserId,
                    p.Nickname,
                    p.Seat,
                    p.IsReady,
                    p.IsConnected
                }).ToList(),
                totalPlayers = game.Players.Count
            });
        }
        
        _sessionHelper.ClearCurrentGameCode();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartGame(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Code is required.");

        code = code.Trim().ToUpperInvariant();

        var game = await _gameRepository.GetByCodeWithPlayersAsync(code);
        if (game == null)
        {
            return NotFound();
        }

        var tempUserId = _sessionHelper.GetTempUserId();
        if (tempUserId == null)
        {
            return RedirectToAction("Index", "Home");
        }

        // Start the game
        await _gameService.StartGameAsync(game);

        // Create the first round
        var round = await _gameService.CreateRoundAsync(game.GameId, 1, game.LeaderSeat);

        await _hubContext.Clients.Group(code).SendAsync("GameStarted", new 
        {
            code,
            roundNumber = game.RoundCounter,
            leaderSeat = game.LeaderSeat
        });

        return RedirectToAction(nameof(Play), new { code });
    }

    [HttpGet]
    public async Task<IActionResult> Play(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Code is required.");
        code = code.Trim().ToUpperInvariant();

        var game = await _gameRepository.GetByCodeWithPlayersAsync(code);
        if (game == null)
        {
            return NotFound();
        }
        if (game.Status != Models.GameStatus.InProgress) //TODO: Change later when status Finished is implemented
        {
            return RedirectToAction(nameof(Lobby), new { code });
        }
        var tempUserId = _sessionHelper.GetTempUserId();
        if (tempUserId == null)
        {
            return RedirectToAction("Index", "Home");
        }
        var player = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId);
        if (player == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var currentRound = await _roundRepository.GetCurrentRoundByGameId(game.GameId, game.RoundCounter);
        var currentLeader = game.Players.FirstOrDefault(p => p.Seat == game.LeaderSeat);

        ViewBag.TempUserId = tempUserId;
        ViewBag.CurrentPlayer = player;
        ViewBag.PlayerNickname = player.Nickname;
        ViewBag.CurrentLeader = currentLeader;
        ViewBag.CurrentRound = currentRound;
        ViewBag.GameCode = code;

        return View(game);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProposeTeam(string code, List<int> selectedSeats)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Code is required.");

        code = code.Trim().ToUpperInvariant();

        var game = await _gameRepository.GetByCodeWithPlayersAndRoundsAsync(code);
        if (game == null)
            return NotFound("Game not found.");

        var tempUserId = _sessionHelper.GetTempUserId();
        if (tempUserId == null)
            return Forbid();

        var leader = game.Players.FirstOrDefault(p => p.Seat == game.LeaderSeat);
        if (leader == null || leader.TempUserId != tempUserId)
            return Forbid("You are not the team leader.");

        var currentRound = game.Rounds.OrderByDescending(r => r.RoundNumber).FirstOrDefault();
        if (currentRound == null)
            return BadRequest("No active round found.");

        if (selectedSeats.Count != currentRound.TeamSize)
        {
            TempData["Error"] = $"You must select exactly {currentRound.TeamSize} player(s).";
            return RedirectToAction(nameof(Play), new { code });
        }

        var teamMembers = game.Players
            .Where(p => selectedSeats.Contains(p.Seat))
            .Select(p => new
            {
                p.TempUserId,
                p.Nickname,
                p.Seat
            })
            .ToList();

        await _hubContext.Clients.Group(code).SendAsync("TeamProposed", new
        {
            leaderSeat = game.LeaderSeat,
            leaderNickname = leader.Nickname,
            teamMembers,
            teamSize = teamMembers.Count
        });

        return RedirectToAction(nameof(Play), new { code });
    }
}
