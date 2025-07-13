using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TicTacToe.Api.Domain.Data;
using TicTacToe.Api.Domain.Models;
using TicTacToe.Api.Domain.Models.DTOs;
using TicTacToe.Api.Domain.Repositories;

public class GameRepositoryTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private IConfiguration GetConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"BOARD_SIZE", "3"},
            {"WIN_CONDITION", "3"},
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task CreateGameAsync_ShouldCreateNewGameSession()
    {
        using var context = GetInMemoryDbContext();
        var configuration = GetConfiguration();
        var repository = new GameRepository(context, configuration);
        var createDto = new CreateGameSessionDto { Probability = 10 };

        var gameSession = await repository.CreateGameAsync(createDto);

        Assert.NotNull(gameSession);
        Assert.True(gameSession.Id > 0);
        Assert.NotNull(gameSession.GameBoard);
        Assert.NotNull(gameSession.GameRules);
        Assert.Equal(3, gameSession.GameBoard.Board.GetLength(0));
        Assert.Equal(10, gameSession.GameRules.Probability);
        Assert.Equal(3, gameSession.GameRules.WinCondition);

        var savedSession = await context.GameSessions
            .Include(gs => gs.GameBoard)
            .Include(gs => gs.GameRules)
            .FirstOrDefaultAsync(gs => gs.Id == gameSession.Id);

        Assert.NotNull(savedSession);
        Assert.Equal(gameSession.Id, savedSession.Id);
        Assert.Equal(gameSession.GameBoard.Id, savedSession.GameBoard.Id);
        Assert.Equal(gameSession.GameRules.Id, savedSession.GameRules.Id);
    }

    [Fact]
    public async Task GetGameAsync_ShouldReturnGameSession_WhenExists()
    {
        using var context = GetInMemoryDbContext();
        var configuration = GetConfiguration();
        var repository = new GameRepository(context, configuration);

        var gameBoard = new GameBoard { Board = new char?[3, 3] };
        var gameRules = new GameRules { Probability = 5, WinCondition = 3 };
        var gameSession = new GameSession
        {
            GameBoard = gameBoard,
            GameRules = gameRules
        };

        context.GameBoards.Add(gameBoard);
        context.GameRules.Add(gameRules);
        context.GameSessions.Add(gameSession);
        await context.SaveChangesAsync();

        var retrievedGameSession = await repository.GetGameAsync(gameSession.Id);

        Assert.NotNull(retrievedGameSession);
        Assert.Equal(gameSession.Id, retrievedGameSession.Id);
        Assert.NotNull(retrievedGameSession.GameBoard);
        Assert.NotNull(retrievedGameSession.GameRules);
        Assert.Equal(gameSession.GameBoard.Id, retrievedGameSession.GameBoard.Id);
        Assert.Equal(gameSession.GameRules.Id, retrievedGameSession.GameRules.Id);
    }

    [Fact]
    public async Task MakeMoveAsync_ShouldProcessPlayerMoveAndUpdateGameSession()
    {
        using var context = GetInMemoryDbContext();
        var configuration = GetConfiguration();
        var repository = new GameRepository(context, configuration);

        var initialBoard = new char?[3, 3];
        var gameBoard = new GameBoard { Board = initialBoard };
        var gameRules = new GameRules { Probability = 0, WinCondition = 3 };
        var gameSession = new GameSession
        {
            GameBoard = gameBoard,
            GameRules = gameRules,
            CurrMove = 'x',
            Version = 1
        };

        context.GameBoards.Add(gameBoard);
        context.GameRules.Add(gameRules);
        context.GameSessions.Add(gameSession);
        await context.SaveChangesAsync();

        var moveDto = new PlayerMoveDto { X = 0, Y = 0 };
        var gameSessionId = gameSession.Id;

        var updatedGameSession = await repository.MakeMoveAsync(gameSessionId, moveDto);

        Assert.NotNull(updatedGameSession);
        Assert.Equal(gameSessionId, updatedGameSession.Id);
        Assert.Equal('x', updatedGameSession.GameBoard.Board[0, 0]);
        Assert.Equal('o', updatedGameSession.CurrMove);
        Assert.Null(updatedGameSession.Winner);
        Assert.Equal(2, updatedGameSession.Version);

        var savedSession = await context.GameSessions
            .Include(gs => gs.GameBoard)
            .FirstOrDefaultAsync(gs => gs.Id == gameSessionId);
        Assert.NotNull(savedSession);
        Assert.Equal('x', savedSession.GameBoard.Board[0, 0]);
        Assert.Equal('o', savedSession.CurrMove);
        Assert.Equal(2, savedSession.Version);
    }
}