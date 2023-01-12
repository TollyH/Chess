using System.Drawing;

namespace Chess
{
    public static class BoardAnalysis
    {
        /// <summary>
        /// Determine whether a square, occupied or not, can be reached by any of the opponents pieces
        /// </summary>
        /// <param name="board">The state of the board to check</param>
        /// <param name="target">The target square on the board</param>
        /// <param name="isWhite">Is the current player (not the opponent!) white?</param>
        public static bool IsSquareOpponentReachable(Pieces.IPiece?[,] board, Point target, bool isWhite)
        {
            // King check
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dy != 0 || dx != 0)
                    {
                        Point newPos = new(target.X + dx, target.Y + dy);
                        if (newPos.X >= 0 && newPos.Y >= 0 && newPos.X < board.GetLength(0) && newPos.Y < board.GetLength(1)
                            && board[newPos.X, newPos.Y] is not null && board[newPos.X, newPos.Y]!.IsWhite != isWhite
                            && board[newPos.X, newPos.Y]!.GetType() == typeof(Pieces.King))
                        {
                            return true;
                        }
                    }
                }
            }

            // Straight checks (rook & queen)

            // Diagonal checks (bishop & queen)

            // Knight checks

            return false;
        }
    }
}
