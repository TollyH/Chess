using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;

namespace Chess
{
    /// <remarks>
    /// CheckWhite and CheckMateWhite mean that the check is against white,
    /// or that white has lost respectively, and vice versa.
    /// </remarks>
    public enum GameState
    {
        StandardPlay,
        DrawStalemate,
        DrawFiftyMove,
        DrawThreeFold,
        DrawInsufficientMaterial,
        CheckWhite,
        CheckBlack,
        CheckMateWhite,
        CheckMateBlack
    }

    public class ChessGame
    {
        public static readonly ImmutableHashSet<GameState> EndingStates = new HashSet<GameState>()
        {
            GameState.DrawFiftyMove,
            GameState.DrawStalemate,
            GameState.DrawThreeFold,
            GameState.DrawInsufficientMaterial,
            GameState.CheckMateWhite,
            GameState.CheckMateBlack
        }.ToImmutableHashSet();

        public Pieces.Piece?[,] Board { get; }

        public Pieces.King WhiteKing { get; }
        public Pieces.King BlackKing { get; }

        public bool CurrentTurnWhite { get; private set; }
        public bool GameOver { get; private set; }

        /// <summary>
        /// A list of the moves made this game as (sourcePosition, destinationPosition)
        /// </summary>
        public List<(Point, Point)> Moves { get; }
        public List<Pieces.Piece> CapturedPieces { get; }

        public bool WhiteMayCastleKingside { get; private set; }
        public bool WhiteMayCastleQueenside { get; private set; }
        public bool BlackMayCastleKingside { get; private set; }
        public bool BlackMayCastleQueenside { get; private set; }

        // Used for the 50-move rule
        public int StaleMoveCounter { get; private set; }
        // Used to detect three-fold repetition
        public Dictionary<string, int> BoardCounts { get; }

        public ChessGame()
        {
            CurrentTurnWhite = true;
            GameOver = false;

            WhiteKing = new Pieces.King(new Point(4, 0), true);
            BlackKing = new Pieces.King(new Point(4, 7), false);

            Moves = new List<(Point, Point)>();
            CapturedPieces = new List<Pieces.Piece>();

            WhiteMayCastleKingside = true;
            WhiteMayCastleQueenside = true;
            BlackMayCastleKingside = true;
            BlackMayCastleQueenside = true;

            StaleMoveCounter = 0;
            BoardCounts = new Dictionary<string, int>();

            Board = new Pieces.Piece?[8, 8]
            {
                { new Pieces.Rook(new Point(0, 0), true), new Pieces.Pawn(new Point(0, 1), true), null, null, null, null, new Pieces.Pawn(new Point(0, 6), false), new Pieces.Rook(new Point(0, 7), false) },
                { new Pieces.Knight(new Point(1, 0), true), new Pieces.Pawn(new Point(1, 1), true), null, null, null, null, new Pieces.Pawn(new Point(1, 6), false), new Pieces.Knight(new Point(1, 7), false) },
                { new Pieces.Bishop(new Point(2, 0), true), new Pieces.Pawn(new Point(2, 1), true), null, null, null, null, new Pieces.Pawn(new Point(2, 6), false), new Pieces.Bishop(new Point(2, 7), false) },
                { new Pieces.Queen(new Point(3, 0), true), new Pieces.Pawn(new Point(3, 1), true), null, null, null, null, new Pieces.Pawn(new Point(3, 6), false), new Pieces.Queen(new Point(3, 7), false) },
                { WhiteKing, new Pieces.Pawn(new Point(4, 1), true), null, null, null, null, new Pieces.Pawn(new Point(4, 6), false), BlackKing },
                { new Pieces.Bishop(new Point(5, 0), true), new Pieces.Pawn(new Point(5, 1), true), null, null, null, null, new Pieces.Pawn(new Point(5, 6), false), new Pieces.Bishop(new Point(5, 7), false) },
                { new Pieces.Knight(new Point(6, 0), true), new Pieces.Pawn(new Point(6, 1), true), null, null, null, null, new Pieces.Pawn(new Point(6, 6), false), new Pieces.Knight(new Point(6, 7), false) },
                { new Pieces.Rook(new Point(7, 0), true), new Pieces.Pawn(new Point(7, 1), true), null, null, null, null, new Pieces.Pawn(new Point(7, 6), false), new Pieces.Rook(new Point(7, 7), false) }
            };
        }

        /// <summary>
        /// Determine the current state of the game.
        /// </summary>
        /// <remarks>
        /// This method is similar to <see cref="BoardAnalysis.DetermineGameState"/>,
        /// however it can also detect the 50-move rule and three-fold repetition.
        /// </remarks>
        public GameState DetermineGameState()
        {
            GameState staticState = BoardAnalysis.DetermineGameState(Board, CurrentTurnWhite);
            if (EndingStates.Contains(staticState))
            {
                return staticState;
            }
            if (BoardCounts.GetValueOrDefault(Board.ChessBoardToString()) >= 3)
            {
                return GameState.DrawThreeFold;
            }
            // 100 because the 50-move rule needs 50 stale moves from *each* player
            if (StaleMoveCounter >= 100)
            {
                return GameState.DrawFiftyMove;
            }
            return staticState;
        }

        /// <summary>
        /// Determine if the player who's turn it is may castle in a given direction on this turn
        /// </summary>
        /// <param name="kingside"><see langword="true"/> if checking kingside, <see langword="false"/> if checking queenside</param>
        /// <remarks>
        /// This method is similar to <see cref="BoardAnalysis.IsCastlePossible"/>,
        /// however it also accounts for whether a king or rook have moved before
        /// </remarks>
        public bool IsCastlePossible(bool kingside)
        {
            if (GameOver)
            {
                return false;
            }

            if (kingside)
            {
                if (CurrentTurnWhite && !WhiteMayCastleKingside)
                {
                    return false;
                }
                if (!CurrentTurnWhite && !BlackMayCastleKingside)
                {
                    return false;
                }
            }
            else
            {
                if (CurrentTurnWhite && !WhiteMayCastleQueenside)
                {
                    return false;
                }
                if (!CurrentTurnWhite && !BlackMayCastleQueenside)
                {
                    return false;
                }
            }

            return BoardAnalysis.IsCastlePossible(Board, CurrentTurnWhite, kingside);
        }

        /// <summary>
        /// Move a piece on the board from a <paramref name="source"/> coordinate to a <paramref name="destination"/> coordinate.
        /// </summary>
        /// <returns><see langword="true"/> if the move was valid and executed, <see langword="false"/> otherwise</returns>
        /// <remarks>This method will check if the move is completely valid. No other validity checks are required.</remarks>
        public bool MovePiece(Point source, Point destination)
        {
            if (GameOver)
            {
                return false;
            }

            Pieces.Piece? piece = Board[source.X, source.Y];
            if (piece is null)
            {
                return false;
            }
            if (piece.IsWhite != CurrentTurnWhite)
            {
                return false;
            }

            bool pieceMoved;
            int homeY = CurrentTurnWhite ? 0 : 7;
            if (piece is Pieces.King && source.X == 4 && destination.Y == homeY
                && ((destination.X == 6 && IsCastlePossible(true))
                    || (destination.X == 2 && IsCastlePossible(false))))
            {
                // King performed castle, move correct rook
                pieceMoved = true;
                if (CurrentTurnWhite)
                {
                    _ = WhiteKing.Move(Board, destination, true);
                }
                else
                {
                    _ = BlackKing.Move(Board, destination, true);
                }
                int rookXPos = destination.X == 2 ? 0 : 7;
                int newRookXPos = destination.X == 2 ? 3 : 5;
                _ = Board[rookXPos, homeY]!.Move(Board, new Point(newRookXPos, homeY), true);
                Board[newRookXPos, homeY] = Board[rookXPos, homeY];
                Board[rookXPos, homeY] = null;
            }
            else
            {
                pieceMoved = piece.Move(Board, destination);
            }

            if (pieceMoved)
            {
                StaleMoveCounter++;
                Moves.Add((source, destination));
                if (Board[destination.X, destination.Y] is not null)
                {
                    CapturedPieces.Add(Board[destination.X, destination.Y]!);
                    StaleMoveCounter = 0;
                }

                foreach (Pieces.Pawn pawn in Board.OfType<Pieces.Pawn>())
                {
                    pawn.LastMoveWasDouble = false;
                }
                if (piece is Pieces.Pawn movedPawn)
                {
                    StaleMoveCounter = 0;
                    if (Math.Abs(destination.Y - source.Y) > 1)
                    {
                        movedPawn.LastMoveWasDouble = true;
                    }
                    // Take pawn after en passant
                    if (destination.X != source.X && Board[destination.X, destination.Y] is null)
                    {
                        CapturedPieces.Add(Board[destination.X, source.Y]!);
                        Board[destination.X, source.Y] = null;
                    }
                }
                else if (piece is Pieces.King)
                {
                    if (piece.IsWhite)
                    {
                        WhiteMayCastleKingside = false;
                        WhiteMayCastleQueenside = false;
                    }
                    else
                    {
                        BlackMayCastleKingside = false;
                        BlackMayCastleQueenside = false;
                    }
                }
                else if (piece is Pieces.Rook)
                {
                    if (piece.IsWhite)
                    {
                        if (source.X == 0)
                        {
                            WhiteMayCastleQueenside = false;
                        }
                        else if (source.X == 7)
                        {
                            WhiteMayCastleKingside = false;
                        }
                    }
                    else
                    {
                        if (source.X == 0)
                        {
                            BlackMayCastleQueenside = false;
                        }
                        else if (source.X == 7)
                        {
                            BlackMayCastleKingside = false;
                        }
                    }
                }

                Board[destination.X, destination.Y] = piece;
                Board[source.X, source.Y] = null;

                string newBoardString = Board.ChessBoardToString();
                if (BoardCounts.ContainsKey(newBoardString))
                {
                    BoardCounts[newBoardString]++;
                }
                else
                {
                    BoardCounts[newBoardString] = 1;
                }

                CurrentTurnWhite = !CurrentTurnWhite;
                if (EndingStates.Contains(DetermineGameState()))
                {
                    GameOver = true;
                }
                return true;
            }

            return false;
        }
    }
}
