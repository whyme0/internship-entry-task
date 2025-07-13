namespace TicTacToe.Api.Domain.Extensions
{
    public static class CurrMoveExtension
    {
        public static char Flip(this char move)
        {
            return move == 'x' ? 'o' : 'x';
        }
    }
}
