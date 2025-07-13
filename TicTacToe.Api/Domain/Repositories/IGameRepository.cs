using System.Text.Json.Serialization.Metadata;
using TicTacToe.Api.Domain.Models;
using TicTacToe.Api.Domain.Models.DTOs;

namespace TicTacToe.Api.Domain.Repositories
{
    public interface IGameRepository
    {
        public Task<GameSession> CreateGameAsync(CreateGameSessionDto dto);
        public Task<GameSession?> GetGameAsync(int id);
        public Task<GameSession> MakeMoveAsync(int id, PlayerMoveDto dto);
        public Task<char?> GetWinnerCharAsync(int id);
    }
}
