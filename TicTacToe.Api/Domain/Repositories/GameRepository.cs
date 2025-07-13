using Microsoft.EntityFrameworkCore;
using TicTacToe.Api.Domain.Data;
using TicTacToe.Api.Domain.Extensions;
using TicTacToe.Api.Domain.Models;
using TicTacToe.Api.Domain.Models.DTOs;

namespace TicTacToe.Api.Domain.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        public GameRepository(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<GameSession> CreateGameAsync(CreateGameSessionDto dto)
        {
            int boardSize = Convert.ToInt32(_configuration["BOARD_SIZE"]);
            int winCondition = Convert.ToInt32(_configuration["WIN_CONDITION"]);

            GameBoard gb = new GameBoard
            {
                Board = new char?[boardSize, boardSize]
            };
            _context.GameBoards.Add(gb);

            GameRules gr = new GameRules
            {
                Probability = dto.Probability,
                WinCondition = winCondition
            };
            _context.GameRules.Add(gr);

            GameSession gs = new GameSession
            {
                GameBoard = gb,
                GameRules = gr
            };
            _context.GameSessions.Add(gs);

            await _context.SaveChangesAsync();

            return gs;
        }

        public async Task<GameSession?> GetGameAsync(int id)
        {
            var gs = await _context.GameSessions
                .Include(gs => gs.GameRules)
                .Include(gs => gs.GameBoard)
                .FirstOrDefaultAsync(gs => gs.Id == id);
                            
            return gs;
        }

        public async Task<GameSession> MakeMoveAsync(int id, PlayerMoveDto dto)
        {
            var gameSession = await GetGameAsync(id);
            
            if (new Random().NextDouble() * 100 <= gameSession!.GameRules.Probability)
            {
                gameSession.GameBoard.Board[dto.X, dto.Y] = gameSession.CurrMove.Flip();
            }
            else
            {
                gameSession.GameBoard.Board[dto.X, dto.Y] = gameSession.CurrMove;
            }

            gameSession.CurrMove = gameSession.CurrMove.Flip();

            gameSession.Winner = WinnerChecker.CheckWinner(gameSession.GameBoard.Board, gameSession.GameRules.WinCondition);

            gameSession.Version++;
            
            try
            {
                _context.GameSessions.Update(gameSession);
                _context.GameBoards.Update(gameSession.GameBoard);
                await _context.SaveChangesAsync();

                return gameSession;
            }
            catch (DbUpdateConcurrencyException)
            {
                return await GetGameAsync(id);
            }
        }

        public async Task<char?> GetWinnerCharAsync(int id)
        {
            var gs = await GetGameAsync(id);
            return gs!.Winner;
        }
    }
}
