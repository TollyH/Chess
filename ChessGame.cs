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

        /// <summary>
        /// A list of the moves made this game as (sourcePosition, destinationPosition)
        /// </summary>
        public List<(Point, Point)> Moves { get; }
        public List<Pieces.Piece> CapturedPieces { get; }

        // Used for the 50-move rule
        public int StaleMoveCounter { get; private set; }
        // Used to detect three-fold repetition
        public Dictionary<string, int> BoardCounts { get; }

        public ChessGame()
        {
            WhiteKing = new Pieces.King(new Point(4, 0), true);
            BlackKing = new Pieces.King(new Point(4, 7), false);

            CurrentTurnWhite = true;

            Moves = new List<(Point, Point)>();
            CapturedPieces = new List<Pieces.Piece>();

            StaleMoveCounter = 0;
            BoardCounts = new Dictionary<string, int>();

            Board = new Pieces.Piece?[8, 8]
            {
                { new Pieces.Rook(new Point(0, 0), true, WhiteKing), new Pieces.Pawn(new Point(0, 1), true, WhiteKing), null, null, null, null, new Pieces.Pawn(new Point(0, 6), false, BlackKing), new Pieces.Rook(new Point(0, 7), false, BlackKing) },
                { new Pieces.Knight(new Point(1, 0), true, WhiteKing), new Pieces.Pawn(new Point(1, 1), true, WhiteKing), null, null, null, null, new Pieces.Pawn(new Point(1, 6), false, BlackKing), new Pieces.Knight(new Point(1, 7), false, BlackKing) },
                { new Pieces.Bishop(new Point(2, 0), true, WhiteKing), new Pieces.Pawn(new Point(2, 1), true, WhiteKing), null, null, null, null, new Pieces.Pawn(new Point(2, 6), false, BlackKing), new Pieces.Bishop(new Point(2, 7), false, BlackKing) },
                { new Pieces.Queen(new Point(3, 0), true, WhiteKing), new Pieces.Pawn(new Point(3, 1), true, WhiteKing), null, null, null, null, new Pieces.Pawn(new Point(3, 6), false, BlackKing), new Pieces.Queen(new Point(3, 7), false, BlackKing) },
                { WhiteKing, new Pieces.Pawn(new Point(4, 1), true, WhiteKing), null, null, null, null, new Pieces.Pawn(new Point(4, 6), false, BlackKing), BlackKing },
                { new Pieces.Bishop(new Point(5, 0), true, WhiteKing), new Pieces.Pawn(new Point(5, 1), true, WhiteKing), null, null, null, null, new Pieces.Pawn(new Point(5, 6), false, BlackKing), new Pieces.Bishop(new Point(5, 7), false, BlackKing) },
                { new Pieces.Knight(new Point(6, 0), true, WhiteKing), new Pieces.Pawn(new Point(6, 1), true, WhiteKing), null, null, null, null, new Pieces.Pawn(new Point(6, 6), false, BlackKing), new Pieces.Knight(new Point(6, 7), false, BlackKing) },
                { new Pieces.Rook(new Point(7, 0), true, WhiteKing), new Pieces.Pawn(new Point(7, 1), true, WhiteKing), null, null, null, null, new Pieces.Pawn(new Point(7, 6), false, BlackKing), new Pieces.Rook(new Point(7, 7), false, BlackKing) }
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
            GameState staticState = BoardAnalysis.DetermineGameState(Board);
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
        /// Move a piece on the board from a <paramref name="source"/> coordinate to a <paramref name="destination"/> coordinate.
        /// </summary>
        /// <returns><see langword="true"/> if the move was valid and executed, <see langword="false"/> otherwise</returns>
        /// <remarks>This method will check if the move is completely valid. No other validity checks are required.</remarks>
        public bool MovePiece(Point source, Point destination)
        {
            Pieces.Piece? piece = Board[source.X, source.Y];
            if (piece is null)
            {
                return false;
            }
            if (piece.IsWhite != CurrentTurnWhite)
            {
                return false;
            }

            bool pieceMoved = piece.Move(Board, destination);
            if (pieceMoved)
            {
                StaleMoveCounter++;
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
                return true;
            }

            return false;
        }
    }
}
