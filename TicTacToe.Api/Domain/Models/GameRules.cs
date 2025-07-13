namespace TicTacToe.Api.Domain.Models
{
    public class GameRules
    {
        public int Id { get; set; }
        public int WinCondition { get; set; } // How much "x" or "o" should be in a row to win
        public int Probability { get; set; }
    }
}
