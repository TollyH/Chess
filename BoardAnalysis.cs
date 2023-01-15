using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Chess
{
    public static class BoardAnalysis
    {
        /// <summary>
        /// Determine whether a king can be reached by any of the opponents pieces
        /// </summary>
        /// <param name="board">The state of the board to check</param>
        /// <param name="isWhite">Is the king to check white?</param>
        /// <param name="target">Override the position of the king to check</param>
        /// <remarks><paramref name="target"/> should always be given if checking a not-yet-peformed king move, as the king's internally stored position will be incorrect.</remarks>
        public static bool IsKingReachable(Pieces.Piece?[,] board, bool isWhite, Point? target = null)
        {
            target ??= board.OfType<Pieces.King>().Where(x => x.IsWhite == isWhite).First().Position;

            // King check
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dy != 0 || dx != 0)
                    {
                        Point newPos = new(target.Value.X + dx, target.Value.Y + dy);
                        if (newPos.X >= 0 && newPos.Y >= 0 && newPos.X < board.GetLength(0) && newPos.Y < board.GetLength(1)
                            && board[newPos.X, newPos.Y] is Pieces.King && board[newPos.X, newPos.Y]!.IsWhite != isWhite)
                        {
                            return true;
                        }
                    }
                }
            }

            // Straight checks (rook & queen)
            for (int dx = target.Value.X + 1; dx < board.GetLength(0); dx++)
            {
                Point newPos = new(dx, target.Value.Y);
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
            for (int dx = target.Value.X - 1; dx >= 0; dx--)
            {
                Point newPos = new(dx, target.Value.Y);
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
            for (int dy = target.Value.Y + 1; dy < board.GetLength(1); dy++)
            {
                Point newPos = new(target.Value.X, dy);
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
            for (int dy = target.Value.Y - 1; dy >= 0; dy--)
            {
                Point newPos = new(target.Value.X, dy);
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
            for (int dif = 1; target.Value.X + dif < board.GetLength(0) && target.Value.Y + dif < board.GetLength(1); dif++)
            {
                Point newPos = new(target.Value.X + dif, target.Value.Y + dif);
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
            for (int dif = 1; target.Value.X - dif >= 0 && target.Value.Y + dif < board.GetLength(1); dif++)
            {
                Point newPos = new(target.Value.X - dif, target.Value.Y + dif);
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
            for (int dif = 1; target.Value.X - dif >= 0 && target.Value.Y - dif >= 0; dif++)
            {
                Point newPos = new(target.Value.X - dif, target.Value.Y - dif);
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
            for (int dif = 1; target.Value.X + dif < board.GetLength(0) && target.Value.Y - dif >= 0; dif++)
            {
                Point newPos = new(target.Value.X + dif, target.Value.Y - dif);
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
                Point newPos = new(target.Value.X + move.X, target.Value.Y + move.Y);
                if (newPos.X >= 0 && newPos.Y >= 0 && newPos.X < board.GetLength(0) && newPos.Y < board.GetLength(1)
                    && board[newPos.X, newPos.Y] is Pieces.Knight && board[newPos.X, newPos.Y]!.IsWhite != isWhite)
                {
                    return true;
                }
            }

            // Pawn checks
            int pawnYDiff = isWhite ? 1 : -1;
            int newY = target.Value.Y + pawnYDiff;
            if (newY < board.GetLength(1) && newY > 0)
            {
                if (board[target.Value.X, target.Value.Y] is null && board[target.Value.X, newY] is Pieces.Pawn
                    && board[target.Value.X, newY]!.IsWhite != isWhite)
                {
                    return true;
                }
                if (board[target.Value.X, target.Value.Y] is not null)
                {
                    if (target.Value.X > 0 && board[target.Value.X - 1, newY] is Pieces.Pawn
                        && board[target.Value.X - 1, newY]!.IsWhite != isWhite)
                    {
                        return true;
                    }
                    if (target.Value.X < board.GetLength(0) - 1 && board[target.Value.X + 1, newY] is Pieces.Pawn
                        && board[target.Value.X + 1, newY]!.IsWhite != isWhite)
                    {
                        return true;
                    }
                }
            }
            newY = target.Value.Y + (pawnYDiff * 2);
            if (newY == (isWhite ? board.GetLength(1) - 2 : 1))
            {
                if (board[target.Value.X, target.Value.Y] is null && board[target.Value.X, target.Value.Y + pawnYDiff] is null
                    && board[target.Value.X, newY] is Pieces.Pawn && board[target.Value.X, newY]!.IsWhite != isWhite)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determine if the player who's turn it is may castle in a given direction on this turn
        /// </summary>
        /// <param name="kingside"><see langword="true"/> if checking kingside, <see langword="false"/> if checking queenside</param>
        /// <remarks>
        /// This method will not consider whether a king or rook has moved before
        /// </remarks>
        public static bool IsCastlePossible(Pieces.Piece?[,] board, bool currentTurnWhite, bool kingside)
        {
            int yPos = currentTurnWhite ? 0 : 7;
            if (IsKingReachable(board, currentTurnWhite, new Point(4, yPos)))
            {
                return false;
            }

            if (kingside)
            {
                Point rookDest = new(5, yPos);
                Point kingDest = new(6, yPos);
                if (board[rookDest.X, yPos] is not null
                    || IsKingReachable(board, currentTurnWhite, rookDest))
                {
                    return false;
                }
                if (board[kingDest.X, yPos] is not null
                    || IsKingReachable(board, currentTurnWhite, kingDest))
                {
                    return false;
                }
                return true;
            }
            else
            {
                Point rookDest = new(3, yPos);
                Point kingDest = new(2, yPos);
                if (board[rookDest.X, yPos] is not null
                    || IsKingReachable(board, currentTurnWhite, rookDest))
                {
                    return false;
                }
                if (board[kingDest.X, yPos] is not null
                    || IsKingReachable(board, currentTurnWhite, kingDest))
                {
                    return false;
                }
                if (board[1, yPos] is not null)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Determine the current state of the game with the given board.
        /// </summary>
        /// <remarks>
        /// This method will not detect states that depend on game history, such as three-fold repetition or the 50-move rule
        /// </remarks>
        public static GameState DetermineGameState(Pieces.Piece?[,] board, bool currentTurnWhite)
        {
            IEnumerable<Pieces.Piece> whitePieces = board.OfType<Pieces.Piece>().Where(p => p.IsWhite);
            IEnumerable<Pieces.Piece> blackPieces = board.OfType<Pieces.Piece>().Where(p => !p.IsWhite);

            bool whiteCheck = IsKingReachable(board, true);
            // White and Black cannot both be in check
            bool blackCheck = !whiteCheck && IsKingReachable(board, false);

            if (currentTurnWhite && !whitePieces.SelectMany(p => p.GetValidMoves(board, true)).Any())
            {
                // Black may only win if they have white king in check, otherwise draw
                return whiteCheck ? GameState.CheckMateWhite : GameState.DrawStalemate;
            }
            if (!currentTurnWhite && !blackPieces.SelectMany(p => p.GetValidMoves(board, true)).Any())
            {
                // White may only win if they have black king in check, otherwise draw
                return blackCheck ? GameState.CheckMateBlack : GameState.DrawStalemate;
            }

            int whitePiecesCount = whitePieces.Count();
            int blackPiecesCount = blackPieces.Count();
            if ((whitePiecesCount == 1 || (whitePiecesCount == 2
                    && whitePieces.Where(p => p is not Pieces.King).First() is Pieces.Bishop or Pieces.Knight))
                && (blackPiecesCount == 1 || (blackPiecesCount == 2
                    && blackPieces.Where(p => p is not Pieces.King).First() is Pieces.Bishop or Pieces.Knight)))
            {
                return GameState.DrawInsufficientMaterial;
            }

            if ((whitePiecesCount == 1 && blackPiecesCount == 3 && blackPieces.OfType<Pieces.Knight>().Count() == 2)
                || (blackPiecesCount == 1 && whitePiecesCount == 3 && whitePieces.OfType<Pieces.Knight>().Count() == 2))
            {
                return GameState.DrawInsufficientMaterial;
            }

            return whiteCheck ? GameState.CheckWhite : blackCheck ? GameState.CheckBlack : GameState.StandardPlay;
        }
    }
}
