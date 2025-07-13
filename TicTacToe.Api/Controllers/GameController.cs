using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using TicTacToe.Api.Domain.Models;
using TicTacToe.Api.Domain.Models.DTOs;
using TicTacToe.Api.Domain.Repositories;

namespace TicTacToe.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {

        private readonly ILogger<GameController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IGameRepository _repository;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
        private readonly IMemoryCache _memoryCache;
        public GameController(ILogger<GameController> logger, IConfiguration configuration, IGameRepository gameRepository, IMemoryCache memoryCache)
        {
            _logger = logger;
            _configuration = configuration;
            _repository = gameRepository;
            _memoryCache = memoryCache;
        }

        [HttpPost("create", Name = "CreateGame")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameSessionDto dto)
        {
            int boardSize = Convert.ToInt32(_configuration["BOARD_SIZE"]);

            var gs = await _repository.CreateGameAsync(dto);

            return CreatedAtAction("GetGame", new { id = gs.Id }, gs);
        }

        [HttpGet("{id}", Name = "GetGame")]
        public async Task<IActionResult> GetGame(int id)
        {
            var gameSession = await _repository.GetGameAsync(id);
            
            if (gameSession == null)
                return NotFound();

            var game = new GameSessionsDto
            {
                Id = gameSession.Id,
                CurrMove = gameSession.CurrMove,
                Winner = gameSession.Winner,
                GameRules = gameSession.GameRules,
                GameBoard = gameSession.GameBoard
            };

            return Ok(game);
        }

        [HttpPut("{id}/move", Name = "MakeMove")]
        public async Task<IActionResult> MakeMove(int id, [FromBody] PlayerMoveDto dto, [FromHeader(Name = "Idempotency-Key")] string ik)
        {
            if (string.IsNullOrEmpty(ik))
                return BadRequest("Idempotency-Key required");

            if (_memoryCache.TryGetValue(ik, out CachedGameSessionResponse cached))
                return OkWithETag(cached.Data, cached.ETag);

            var semaphore = _locks.GetOrAdd(ik, new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();

            try
            {
                if (_memoryCache.TryGetValue(ik, out cached))
                    return OkWithETag(cached.Data, cached.ETag);

                var gameSession = await _repository.GetGameAsync(id);
                
                if (gameSession == null)
                    return NotFound();

                var ifMatch = Request.Headers.IfMatch.FirstOrDefault();
                if (ifMatch != null && ifMatch != $"\"{gameSession.Version}\"")
                    return StatusCode(412);

                if (gameSession.Winner != null)
                    return BadRequest("Cannot make move in a completed game");

                if (gameSession.GameBoard.Board[dto.X, dto.Y] != null)
                    return BadRequest("Cannot make move to filled cell");

                gameSession = await _repository.MakeMoveAsync(id, dto);

                _memoryCache.Set(ik, new CachedGameSessionResponse {
                    Data = gameSession,
                    ETag = gameSession.Version.ToString()
                }, TimeSpan.FromDays(1));

                return OkWithETag(new GameSessionsDto
                {
                    Id = gameSession.Id,
                    CurrMove = gameSession.CurrMove,
                    Winner = gameSession.Winner,
                    GameRules = gameSession.GameRules,
                    GameBoard = gameSession.GameBoard
                }, gameSession.Version.ToString());
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(412);
            }
            finally
            {
                semaphore.Release();
            }
        }

        [HttpGet("{id}/winner", Name = "GetWinner")]
        public async Task<IActionResult> GetWinner(int id)
        {
            if(await _repository.GetGameAsync(id) == null)
                return NotFound();
            
            return Ok(new { Winner = await _repository.GetWinnerCharAsync(id) });
        }

        private IActionResult OkWithETag(object body, string etag)
        {
            Response.Headers.ETag = $"\"{etag}\"";
            return Ok(body);
        }

        private class CachedGameSessionResponse
        {
            public GameSession Data { get; set; }
            public string ETag { get; set; }
        }
    }
}
