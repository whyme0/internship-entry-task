namespace TicTacToe.Api.Domain.Models.DTOs
{
    public class GameSessionsDto
    {
        public int Id { get; set; }
        public char CurrMove { get; set; }
        public char? Winner { get; set; }
        public GameRules GameRules { get; set; }
        public GameBoard GameBoard { get; set; }
    }
}
