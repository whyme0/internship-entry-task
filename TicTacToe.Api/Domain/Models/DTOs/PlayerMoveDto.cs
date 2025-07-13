using System.ComponentModel.DataAnnotations;

namespace TicTacToe.Api.Domain.Models.DTOs
{
    public class PlayerMoveDto
    {
        [Range(0, 2, ErrorMessage = "Coordinate X must be between 0 and 2.")]
        public int X { get; set; }
        [Range(0, 2, ErrorMessage = "Coordinate Y must be between 0 and 2.")]
        public int Y { get; set; }
    }
}
