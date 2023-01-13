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
                            && board[newPos.X, newPos.Y] is Pieces.King && board[newPos.X, newPos.Y]!.IsWhite != isWhite)
                        {
                            return true;
                        }
                    }
                }
            }

            // Straight checks (rook & queen)
            for (int dx = target.X + 1; dx < board.GetLength(0); dx++)
            {
                Point newPos = new(dx, target.Y);
                if (board[newPos.X, newPos.Y] is not null)
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != isWhite &&
                        board[newPos.X, newPos.Y] is Pieces.Queen or Pieces.Rook)
                    {
                        return true;
                    }
                    break;
                }
            }
            for (int dx = target.X - 1; dx >= 0; dx--)
            {
                Point newPos = new(dx, target.Y);
                if (board[newPos.X, newPos.Y] is not null)
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != isWhite &&
                        board[newPos.X, newPos.Y] is Pieces.Queen or Pieces.Rook)
                    {
                        return true;
                    }
                    break;
                }
            }
            for (int dy = target.Y + 1; dy < board.GetLength(1); dy++)
            {
                Point newPos = new(target.X, dy);
                if (board[newPos.X, newPos.Y] is not null)
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != isWhite &&
                        board[newPos.X, newPos.Y] is Pieces.Queen or Pieces.Rook)
                    {
                        return true;
                    }
                    break;
                }
            }
            for (int dy = target.Y - 1; dy >= 0; dy--)
            {
                Point newPos = new(target.X, dy);
                if (board[newPos.X, newPos.Y] is not null)
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != isWhite &&
                        board[newPos.X, newPos.Y] is Pieces.Queen or Pieces.Rook)
                    {
                        return true;
                    }
                    break;
                }
            }

            // Diagonal checks (bishop & queen)
            for (int dif = 1; target.X + dif < board.GetLength(0) && target.Y + dif < board.GetLength(1); dif++)
            {
                Point newPos = new(target.X + dif, target.Y + dif);
                if (board[newPos.X, newPos.Y] is not null)
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != isWhite &&
                        board[newPos.X, newPos.Y] is Pieces.Queen or Pieces.Bishop)
                    {
                        return true;
                    }
                    break;
                }
            }
            for (int dif = 1; target.X - dif >= 0 && target.Y + dif < board.GetLength(1); dif++)
            {
                Point newPos = new(target.X - dif, target.Y + dif);
                if (board[newPos.X, newPos.Y] is not null)
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != isWhite &&
                        board[newPos.X, newPos.Y] is Pieces.Queen or Pieces.Bishop)
                    {
                        return true;
                    }
                    break;
                }
            }
            for (int dif = 1; target.X - dif >= 0 && target.Y - dif >= 0; dif++)
            {
                Point newPos = new(target.X - dif, target.Y - dif);
                if (board[newPos.X, newPos.Y] is not null)
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != isWhite &&
                        board[newPos.X, newPos.Y] is Pieces.Queen or Pieces.Bishop)
                    {
                        return true;
                    }
                    break;
                }
            }
            for (int dif = 1; target.X + dif < board.GetLength(0) && target.Y - dif >= 0; dif++)
            {
                Point newPos = new(target.X + dif, target.Y - dif);
                if (board[newPos.X, newPos.Y] is not null)
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != isWhite &&
                        board[newPos.X, newPos.Y] is Pieces.Queen or Pieces.Bishop)
                    {
                        return true;
                    }
                    break;
                }
            }

            // Knight checks
            foreach (Point move in Pieces.Knight.Moves)
            {
                Point newPos = new(target.X + move.X, target.Y + move.Y);
                if (newPos.X >= 0 && newPos.Y >= 0 && newPos.X < board.GetLength(0) && newPos.Y < board.GetLength(1)
                    && board[newPos.X, newPos.Y] is Pieces.Knight && board[newPos.X, newPos.Y]!.IsWhite != isWhite)
                {
                    return true;
                }
            }

            // Pawn checks
            int pawnYDiff = isWhite ? -1 : 1;
            int newY = target.Y + pawnYDiff;
            if (newY < board.GetLength(1) && newY > 0)
            {
                if (board[target.X, target.Y] is null && board[target.X, newY] is Pieces.Pawn
                    && board[target.X, newY]!.IsWhite != isWhite)
                {
                    return true;
                }
                if (board[target.X, target.Y] is not null
                    // En passant
                    || (target.Y - pawnYDiff < board.GetLength(1) && target.Y - pawnYDiff > 0
                        && board[target.X, target.Y - pawnYDiff] is Pieces.Pawn
                        && ((Pieces.Pawn)board[target.X, target.Y - pawnYDiff]!).LastMoveWasDouble))
                {
                    if (target.X > 0 && board[target.X - 1, newY] is Pieces.Pawn
                        && board[target.X - 1, newY]!.IsWhite != isWhite)
                    {
                        return true;
                    }
                    if (target.X < board.GetLength(0) - 1 && board[target.X + 1, newY] is Pieces.Pawn
                        && board[target.X + 1, newY]!.IsWhite != isWhite)
                    {
                        return true;
                    }
                }
            }
            newY = target.Y + (pawnYDiff * 2);
            if (newY == (isWhite ? board.GetLength(1) - 2 : 1))
            {
                if (board[target.X, target.Y] is null && board[target.X, target.Y + pawnYDiff] is null
                    && board[target.X, newY] is Pieces.Pawn && board[target.X, newY]!.IsWhite != isWhite)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
