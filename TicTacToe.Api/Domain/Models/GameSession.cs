namespace TicTacToe.Api.Domain.Models
{
    public class GameSession
    {
        public int Id { get; set; }
        public char CurrMove { get; set; }
        public char? Winner { get; set; } // 'd' if draw
        public int Version { get; set; }
        public int GameRulesId { get; set; }
        public GameRules GameRules { get; set; }
        public int GameBoardId { get; set; }
        public GameBoard GameBoard {get;set;}
    }
}
