namespace TicTacToe.Api.Domain.Models
{
    public class GameBoard
    {
        public int Id { get; set; }
        public char?[,] Board { get; set; }
    }
}
