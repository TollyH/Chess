using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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
        public static bool IsSquareOpponentReachable(Pieces.Piece?[,] board, Point target, bool isWhite)
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

        /// <summary>
        /// Determine the current state of the game with the given board.
        /// </summary>
        /// <remarks>
        /// This method will not detect states that depend on game history, such as three-fold repetition or the 50-move rule
        /// </remarks>
        public static GameState DetermineGameState(Pieces.Piece?[,] board)
        {
            IEnumerable<Pieces.Piece> whitePieces = board.OfType<Pieces.Piece>().Where(p => p.IsWhite);
            IEnumerable<Pieces.Piece> blackPieces = board.OfType<Pieces.Piece>().Where(p => !p.IsWhite);

            Pieces.King whiteKing = whitePieces.OfType<Pieces.King>().First();
            Pieces.King blackKing = blackPieces.OfType<Pieces.King>().First();

            HashSet<Point> whiteMoves = whitePieces.SelectMany(p => p.GetValidMoves(board, true)).ToHashSet();
            HashSet<Point> blackMoves = blackPieces.SelectMany(p => p.GetValidMoves(board, true)).ToHashSet();

            bool whiteCheck = IsSquareOpponentReachable(board, whiteKing.Position, true);
            // White and Black cannot both be in check
            bool blackCheck = !whiteCheck && IsSquareOpponentReachable(board, blackKing.Position, false);

            if (!whiteMoves.Any())
            {
                // Black may only win if they have white king in check, otherwise draw
                return whiteCheck ? GameState.CheckMateWhite : GameState.DrawStalemate;
            }
            if (!blackMoves.Any())
            {
                // White may only win if they have black king in check, otherwise draw
                return blackCheck ? GameState.CheckMateBlack : GameState.DrawStalemate;
            }

            if ((whitePieces.Count() == 1 || (whitePieces.Count() == 2
                    && whitePieces.Where(p => p is not Pieces.King).First() is Pieces.Bishop or Pieces.Knight))
                && (blackPieces.Count() == 1 || (blackPieces.Count() == 2
                    && blackPieces.Where(p => p is not Pieces.King).First() is Pieces.Bishop or Pieces.Knight)))
            {
                return GameState.DrawInsufficientMaterial;
            }

            if ((whitePieces.Count() == 1 && blackPieces.Count() == 3 && blackPieces.OfType<Pieces.Knight>().Count() == 2)
                || (blackPieces.Count() == 1 && whitePieces.Count() == 3 && whitePieces.OfType<Pieces.Knight>().Count() == 2))
            {
                return GameState.DrawInsufficientMaterial;
            }

            return whiteCheck ? GameState.CheckWhite : blackCheck ? GameState.CheckBlack : GameState.StandardPlay;
        }
    }
}
