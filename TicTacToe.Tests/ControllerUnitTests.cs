using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TicTacToe.Api.Controllers;
using TicTacToe.Api.Domain.Repositories;
using TicTacToe.Api.Domain.Models;
using TicTacToe.Api.Domain.Models.DTOs;

public class ControllerTests
{
    private readonly Mock<IGameRepository> _mockRepo;
    private readonly Mock<ILogger<GameController>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly IMemoryCache _memoryCache;
    private readonly GameController _controller;

    public ControllerTests()
    {
        _mockRepo = new Mock<IGameRepository>();
        _mockLogger = new Mock<ILogger<GameController>>();
        _mockConfig = new Mock<IConfiguration>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _controller = new GameController(
            _mockLogger.Object,
            _mockConfig.Object,
            _mockRepo.Object,
            _memoryCache
        );

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task MakeMove_WhenGameNotFound_ReturnsNotFound()
    {
        var gameId = 1;
        _mockRepo.Setup(repo => repo.GetGameAsync(gameId)).ReturnsAsync((GameSession)null);

        var result = await _controller.MakeMove(gameId, new PlayerMoveDto(), "some-key");

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task MakeMove_WhenIfMatchHeaderIsStale_ReturnsPreconditionFailed()
    {
        var gameId = 1;
        var game = new GameSession { Id = gameId, Version = 2 };
        _mockRepo.Setup(repo => repo.GetGameAsync(gameId)).ReturnsAsync(game);

        _controller.Request.Headers["If-Match"] = "\"1\"";

        var result = await _controller.MakeMove(gameId, new PlayerMoveDto(), "some-key");

        var statusCodeResult = result.Should().BeOfType<StatusCodeResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(412);
    }

    [Fact]
    public async Task MakeMove_WithValidMove_ReturnsOkWithCorrectETag()
    {
        var gameId = 1;
        var moveDto = new PlayerMoveDto { X = 0, Y = 0 };
        var idempotencyKey = "unique-key-123";

        var gameBeforeMove = new GameSession { Id = gameId, Version = 1, GameBoard = new GameBoard { Board = new char?[3, 3] } };
        var gameAfterMove = new GameSession { Id = gameId, Version = 2, CurrMove = 'O' };

        _mockRepo.Setup(repo => repo.GetGameAsync(gameId)).ReturnsAsync(gameBeforeMove);
        _mockRepo.Setup(repo => repo.MakeMoveAsync(gameId, moveDto)).ReturnsAsync(gameAfterMove);

        var result = await _controller.MakeMove(gameId, moveDto, idempotencyKey);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<GameSessionsDto>();
        _controller.Response.Headers.ETag.ToString().Should().Be($"\"{gameAfterMove.Version}\"");
    }
}
