using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chess
{
    public static class BoardAnalysis
    {
        private static readonly Random rng = new();

        /// <summary>
        /// Determine whether a king can be reached by any of the opponents pieces
        /// </summary>
        /// <param name="board">The state of the board to check</param>
        /// <param name="isWhite">Is the king to check white?</param>
        /// <param name="target">Override the position of the king to check</param>
        /// <remarks><paramref name="target"/> should always be given if checking a not-yet-performed king move, as the king's internally stored position will be incorrect.</remarks>
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
            if (newY < board.GetLength(1) && newY >= 0)
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
            int yPos = currentTurnWhite ? 0 : board.GetLength(1) - 1;
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
        public static GameState DetermineGameState(Pieces.Piece?[,] board, bool currentTurnWhite,
            Point? whiteKingPos = null, Point? blackKingPos = null)
        {
            IEnumerable<Pieces.Piece> whitePieces = board.OfType<Pieces.Piece>().Where(p => p.IsWhite);
            IEnumerable<Pieces.Piece> blackPieces = board.OfType<Pieces.Piece>().Where(p => !p.IsWhite);

            bool whiteCheck = IsKingReachable(board, true, whiteKingPos ?? null);
            // White and Black cannot both be in check
            bool blackCheck = !whiteCheck && IsKingReachable(board, false, blackKingPos ?? null);

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

        /// <summary>
        /// Calculate the value of the given board based on the remaining pieces
        /// </summary>
        /// <returns>
        /// A <see cref="double"/> representing the total piece value of the entire board.
        /// Positive means white has stronger material, negative means black does.
        /// </returns>
        public static double CalculateBoardValue(Pieces.Piece?[,] board)
        {
            return board.OfType<Pieces.Piece>().Sum(p => p.IsWhite ? p.Value : -p.Value);
        }

        public readonly struct PossibleMove
        {
            public Point Source { get; }
            public Point Destination { get; }
            public double EvaluatedFutureValue { get; }
            public bool WhiteMateLocated { get; }
            public bool BlackMateLocated { get; }
            public int DepthToWhiteMate { get; }
            public int DepthToBlackMate { get; }
            public Type? PromotionType { get; }
            public List<(Point, Point, Type)> BestLine { get;  }

            public PossibleMove(Point source, Point destination, double evaluatedFutureValue,
                bool whiteMateLocated, bool blackMateLocated, int depthToWhiteMate, int depthToBlackMate,
                Type? promotionType, List<(Point, Point, Type)> bestLine)
            {
                Source = source;
                Destination = destination;
                EvaluatedFutureValue = evaluatedFutureValue;
                WhiteMateLocated = whiteMateLocated;
                BlackMateLocated = blackMateLocated;
                DepthToWhiteMate = depthToWhiteMate;
                DepthToBlackMate = depthToBlackMate;
                PromotionType = promotionType;
                BestLine = bestLine;
            }
        }

        /// <summary>
        /// Use <see cref="EvaluatePossibleMoves"/> to find the best possible move in the current state of the game
        /// </summary>
        /// <param name="maxDepth">The maximum number of half-moves in the future to search</param>
        /// <param name="randomise">Whether or not to randomise the order of moves that have the same score</param>
        public static async Task<PossibleMove> EstimateBestPossibleMove(ChessGame game, int maxDepth, bool randomise, CancellationToken cancellationToken)
        {
            PossibleMove[] moves = await EvaluatePossibleMoves(game, maxDepth, randomise, cancellationToken);
            PossibleMove bestMove = new(default, default,
                game.CurrentTurnWhite ? double.NegativeInfinity : double.PositiveInfinity, false, false, 0, 0, typeof(Pieces.Queen), new());
            foreach (PossibleMove potentialMove in moves)
            {
                if (game.CurrentTurnWhite)
                {
                    if (bestMove.EvaluatedFutureValue == double.NegativeInfinity
                        || (!bestMove.BlackMateLocated && potentialMove.BlackMateLocated)
                        || (!bestMove.BlackMateLocated && potentialMove.EvaluatedFutureValue > bestMove.EvaluatedFutureValue)
                        || (bestMove.BlackMateLocated && potentialMove.BlackMateLocated
                            && potentialMove.DepthToBlackMate < bestMove.DepthToBlackMate))
                    {
                        bestMove = potentialMove;
                    }
                }
                else
                {
                    if (bestMove.EvaluatedFutureValue == double.PositiveInfinity
                        || (!bestMove.WhiteMateLocated && potentialMove.WhiteMateLocated)
                        || (!bestMove.WhiteMateLocated && potentialMove.EvaluatedFutureValue < bestMove.EvaluatedFutureValue)
                        || (bestMove.WhiteMateLocated && potentialMove.WhiteMateLocated
                            && potentialMove.DepthToWhiteMate < bestMove.DepthToWhiteMate))
                    {
                        bestMove = potentialMove;
                    }
                }
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return default;
            }
            return bestMove;
        }

        /// <summary>
        /// Evaluate each possible move in the current state of the game
        /// </summary>
        /// <param name="maxDepth">The maximum number of half-moves in the future to search</param>
        /// <param name="randomise">Whether or not to randomise the order of moves that have the same score</param>
        /// <returns>An array of all possible moves, with information on board value and ability to checkmate</returns>
        public static async Task<PossibleMove[]> EvaluatePossibleMoves(ChessGame game, int maxDepth, bool randomise, CancellationToken cancellationToken)
        {
            List<Task<PossibleMove>> evaluationTasks = new();

            foreach (Pieces.Piece? piece in game.Board)
            {
                if (piece is not null)
                {
                    if (piece.IsWhite != game.CurrentTurnWhite)
                    {
                        continue;
                    }

                    foreach (Point validMove in GetValidMovesForEval(game, piece))
                    {
                        Point thisMove = validMove;
                        evaluationTasks.Add(Task.Run(() =>
                        {
                            ChessGame gameClone = game.Clone(false);
                            List<(Point, Point, Type)> thisLine = new() { (piece.Position, thisMove, typeof(Pieces.Queen)) };
                            _ = gameClone.MovePiece(piece.Position, thisMove, true,
                                promotionType: typeof(Pieces.Queen), updateMoveText: false);

                            PossibleMove bestSubMove = MinimaxMove(gameClone,
                                double.NegativeInfinity, double.PositiveInfinity, 1, maxDepth, thisLine, cancellationToken);

                            return new PossibleMove(piece.Position, thisMove, bestSubMove.EvaluatedFutureValue,
                                bestSubMove.WhiteMateLocated, bestSubMove.BlackMateLocated,
                                bestSubMove.DepthToWhiteMate, bestSubMove.DepthToBlackMate, typeof(Pieces.Queen), bestSubMove.BestLine);
                        }, cancellationToken));
                    }
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Array.Empty<PossibleMove>();
            }
            try
            {
                IEnumerable<PossibleMove> moves =
                    (await Task.WhenAll(evaluationTasks)).Where(m => m.Source != m.Destination);
                if (randomise)
                {
                    return moves.OrderBy(_ => rng.Next()).ToArray();
                }
                // Remove default moves from return value
                return moves.ToArray();
            }
            catch (TaskCanceledException)
            {
                return Array.Empty<PossibleMove>();
            }
        }

        private static HashSet<Point> GetValidMovesForEval(ChessGame game, Pieces.Piece piece)
        {
            HashSet<Point> allValidMoves = piece.GetValidMoves(game.Board, true);

            if (piece is Pieces.King)
            {
                int homeY = game.CurrentTurnWhite ? 0 : game.Board.GetLength(1) - 1;
                if (game.IsCastlePossible(true))
                {
                    _ = allValidMoves.Add(new Point(6, homeY));
                }
                if (game.IsCastlePossible(false))
                {
                    _ = allValidMoves.Add(new Point(2, homeY));
                }
            }
            else if (piece is Pieces.Pawn && game.EnPassantSquare is not null
                && Math.Abs(piece.Position.X - game.EnPassantSquare.Value.X) == 1
                && piece.Position.Y == (game.CurrentTurnWhite ? 4 : 3)
                && !IsKingReachable(game.Board.AfterMove(piece.Position,
                        game.EnPassantSquare.Value), game.CurrentTurnWhite))
            {
                _ = allValidMoves.Add(game.EnPassantSquare.Value);
            }

            return allValidMoves;
        }

        private static PossibleMove MinimaxMove(ChessGame game, double alpha, double beta, int depth, int maxDepth,
            List<(Point, Point, Type)> currentLine, CancellationToken cancellationToken)
        {
            (Point, Point) lastMove = game.Moves.Last();
            if (game.GameOver)
            {
                GameState state = game.DetermineGameState();
                if (state == GameState.CheckMateWhite)
                {
                    return new PossibleMove(lastMove.Item1, lastMove.Item2, double.NegativeInfinity, true, false, depth, 0, typeof(Pieces.Queen),
                        currentLine);
                }
                else if (state == GameState.CheckMateBlack)
                {
                    return new PossibleMove(lastMove.Item1, lastMove.Item2, double.PositiveInfinity, false, true, 0, depth, typeof(Pieces.Queen),
                        currentLine);
                }
                else
                {
                    // Draw
                    return new PossibleMove(lastMove.Item1, lastMove.Item2, 0, false, false, 0, 0, typeof(Pieces.Queen), currentLine);
                }
            }
            if (depth > maxDepth)
            {
                return new PossibleMove(lastMove.Item1, lastMove.Item2, CalculateBoardValue(game.Board), false, false, 0, 0, typeof(Pieces.Queen), currentLine);
            }

            PossibleMove bestMove = new(default, default,
                game.CurrentTurnWhite ? double.NegativeInfinity : double.PositiveInfinity, false, false, 0, 0, typeof(Pieces.Queen), new());

            foreach (Pieces.Piece? piece in game.Board)
            {
                if (piece is not null)
                {
                    if (piece.IsWhite != game.CurrentTurnWhite)
                    {
                        continue;
                    }

                    foreach (Point validMove in GetValidMovesForEval(game, piece))
                    {
                        ChessGame gameClone = game.Clone(false);
                        List<(Point, Point, Type)> newLine = new(currentLine) { (piece.Position, validMove, typeof(Pieces.Queen)) };
                        _ = gameClone.MovePiece(piece.Position, validMove, true,
                            promotionType: typeof(Pieces.Queen), updateMoveText: false);
                        PossibleMove potentialMove = MinimaxMove(gameClone, alpha, beta, depth + 1, maxDepth, newLine, cancellationToken);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return bestMove;
                        }
                        if (game.CurrentTurnWhite)
                        {
                            if (bestMove.EvaluatedFutureValue == double.NegativeInfinity
                                || (!bestMove.BlackMateLocated && potentialMove.BlackMateLocated)
                                || (!bestMove.BlackMateLocated && potentialMove.EvaluatedFutureValue > bestMove.EvaluatedFutureValue)
                                || (bestMove.BlackMateLocated && potentialMove.BlackMateLocated
                                    && potentialMove.DepthToBlackMate < bestMove.DepthToBlackMate))
                            {
                                bestMove = new PossibleMove(piece.Position, validMove, potentialMove.EvaluatedFutureValue,
                                    potentialMove.WhiteMateLocated, potentialMove.BlackMateLocated,
                                    potentialMove.DepthToWhiteMate, potentialMove.DepthToBlackMate, typeof(Pieces.Queen), potentialMove.BestLine);
                            }
                            if (potentialMove.EvaluatedFutureValue >= beta && !bestMove.BlackMateLocated)
                            {
                                return bestMove;
                            }
                            if (potentialMove.EvaluatedFutureValue > alpha)
                            {
                                alpha = potentialMove.EvaluatedFutureValue;
                            }
                        }
                        else
                        {
                            if (bestMove.EvaluatedFutureValue == double.PositiveInfinity
                                || (!bestMove.WhiteMateLocated && potentialMove.WhiteMateLocated)
                                || (!bestMove.WhiteMateLocated && potentialMove.EvaluatedFutureValue < bestMove.EvaluatedFutureValue)
                                || (bestMove.WhiteMateLocated && potentialMove.WhiteMateLocated
                                    && potentialMove.DepthToWhiteMate < bestMove.DepthToWhiteMate))
                            {
                                bestMove = new PossibleMove(piece.Position, validMove, potentialMove.EvaluatedFutureValue,
                                    potentialMove.WhiteMateLocated, potentialMove.BlackMateLocated,
                                    potentialMove.DepthToWhiteMate, potentialMove.DepthToBlackMate, typeof(Pieces.Queen), potentialMove.BestLine);
                            }
                            if (potentialMove.EvaluatedFutureValue <= alpha && !bestMove.WhiteMateLocated)
                            {
                                return bestMove;
                            }
                            if (potentialMove.EvaluatedFutureValue < beta)
                            {
                                beta = potentialMove.EvaluatedFutureValue;
                            }
                        }
                    }
                }
            }

            return bestMove;
        }
    }
}
