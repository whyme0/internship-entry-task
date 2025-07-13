namespace TicTacToe.Api.Domain.Extensions
{
    public static class WinnerChecker
    {
        public static char? CheckWinner(char?[,] board, int m)
        {
            int n = board.GetLength(0);
            bool hasEmptyCell = false;

            for (int row = 0; row < n; row++)
            {
                for (int col = 0; col < n; col++)
                {
                    char? current = board[row, col];
                    if (current == null)
                    {
                        hasEmptyCell = true;
                        continue;
                    }

                    if (col <= n - m)
                    {
                        bool win = true;
                        for (int k = 1; k < m; k++)
                        {
                            if (board[row, col + k] != current)
                            {
                                win = false;
                                break;
                            }
                        }
                        if (win) return current;
                    }

                    if (row <= n - m)
                    {
                        bool win = true;
                        for (int k = 1; k < m; k++)
                        {
                            if (board[row + k, col] != current)
                            {
                                win = false;
                                break;
                            }
                        }
                        if (win) return current;
                    }

                    if (row <= n - m && col <= n - m)
                    {
                        bool win = true;
                        for (int k = 1; k < m; k++)
                        {
                            if (board[row + k, col + k] != current)
                            {
                                win = false;
                                break;
                            }
                        }
                        if (win) return current;
                    }

                    if (row >= m - 1 && col <= n - m)
                    {
                        bool win = true;
                        for (int k = 1; k < m; k++)
                        {
                            if (board[row - k, col + k] != current)
                            {
                                win = false;
                                break;
                            }
                        }
                        if (win) return current;
                    }
                }
            }

            if (!hasEmptyCell)
                return 'd';

            return null;
        }
    }
}
