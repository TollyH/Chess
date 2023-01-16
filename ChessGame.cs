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

        public Point? EnPassantSquare { get; private set; }
        public bool WhiteMayCastleKingside { get; private set; }
        public bool WhiteMayCastleQueenside { get; private set; }
        public bool BlackMayCastleKingside { get; private set; }
        public bool BlackMayCastleQueenside { get; private set; }

        // Used for the 50-move rule
        public int StaleMoveCounter { get; private set; }
        // Used to detect three-fold repetition
        public Dictionary<string, int> BoardCounts { get; }

        /// <summary>
        /// Create a new standard chess game with all values at their defaults
        /// </summary>
        public ChessGame()
        {
            CurrentTurnWhite = true;
            GameOver = false;

            WhiteKing = new Pieces.King(new Point(4, 0), true);
            BlackKing = new Pieces.King(new Point(4, 7), false);

            Moves = new List<(Point, Point)>();
            CapturedPieces = new List<Pieces.Piece>();

            EnPassantSquare = null;
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
        /// Create a new instance of a chess game, setting each game parameter to a non-default value
        /// </summary>
        public ChessGame(Pieces.Piece?[,] board, bool currentTurnWhite, bool gameOver, List<(Point, Point)> moves,
            List<Pieces.Piece> capturedPieces, Point? enPassantSquare, bool whiteMayCastleKingside, bool whiteMayCastleQueenside,
            bool blackMayCastleKingside, bool blackMayCastleQueenside, int staleMoveCounter, Dictionary<string, int> boardCounts)
        {
            Board = board;
            WhiteKing = Board.OfType<Pieces.King>().Where(k => k.IsWhite).First();
            BlackKing = Board.OfType<Pieces.King>().Where(k => !k.IsWhite).First();

            CurrentTurnWhite = currentTurnWhite;
            GameOver = gameOver;
            Moves = moves;
            CapturedPieces = capturedPieces;
            EnPassantSquare = enPassantSquare;
            WhiteMayCastleKingside = whiteMayCastleKingside;
            WhiteMayCastleQueenside = whiteMayCastleQueenside;
            BlackMayCastleKingside = blackMayCastleKingside;
            BlackMayCastleQueenside = blackMayCastleQueenside;
            StaleMoveCounter = staleMoveCounter;
            BoardCounts = boardCounts;
        }

        /// <summary>
        /// Create a deep copy of all parameters to this chess game
        /// </summary>
        public ChessGame Clone()
        {
            Pieces.Piece?[,] boardClone = new Pieces.Piece?[Board.GetLength(0), Board.GetLength(1)];
            for (int x = 0; x < boardClone.GetLength(0); x++)
            {
                for (int y = 0; y < boardClone.GetLength(1); y++)
                {
                    boardClone[x, y] = Board[x, y]?.Clone();
                }
            }

            return new ChessGame(boardClone, CurrentTurnWhite, GameOver, new(Moves),
                CapturedPieces.Select(c => c.Clone()).ToList(), EnPassantSquare, WhiteMayCastleKingside,
                WhiteMayCastleQueenside, BlackMayCastleKingside, BlackMayCastleQueenside, StaleMoveCounter,
                new(BoardCounts));
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
        /// <remarks>This method will check if the move is completely valid, unless <paramref name="forceMove"/> is <see langword="true"/>. No other validity checks are required.</remarks>
        public bool MovePiece(Point source, Point destination, bool forceMove = false)
        {
            if (!forceMove && GameOver)
            {
                return false;
            }

            Pieces.Piece? piece = Board[source.X, source.Y];
            if (piece is null)
            {
                return false;
            }
            if (!forceMove && piece.IsWhite != CurrentTurnWhite)
            {
                return false;
            }

            bool pieceMoved;
            int homeY = CurrentTurnWhite ? 0 : 7;
            if (piece is Pieces.King && source.X == 4 && destination.Y == homeY
                && ((destination.X == 6 && (forceMove || IsCastlePossible(true)))
                    || (destination.X == 2 && (forceMove || IsCastlePossible(false)))))
            {
                // King performed castle, move correct rook
                pieceMoved = true;
                _ = piece.Move(Board, destination, true);

                int rookXPos = destination.X == 2 ? 0 : 7;
                int newRookXPos = destination.X == 2 ? 3 : 5;
                _ = Board[rookXPos, homeY]!.Move(Board, new Point(newRookXPos, homeY), true);
                Board[newRookXPos, homeY] = Board[rookXPos, homeY];
                Board[rookXPos, homeY] = null;
            }
            else if (piece is Pieces.Pawn && destination == EnPassantSquare && (forceMove ||
                (Math.Abs(source.X - destination.X) == 1 && source.Y == (CurrentTurnWhite ? 4 : 3)
                && !BoardAnalysis.IsKingReachable(Board.AfterMove(source, destination), CurrentTurnWhite))))
            {
                pieceMoved = true;
                _ = piece.Move(Board, destination, true);
                // Take pawn after en passant
                CapturedPieces.Add(Board[destination.X, source.Y]!);
                Board[destination.X, source.Y] = null;
            }
            else
            {
                pieceMoved = piece.Move(Board, destination, forceMove);
            }

            if (pieceMoved)
            {
                StaleMoveCounter++;
                Moves.Add((source, destination));
                if (Board[destination.X, destination.Y] is not null)
                {
                    if (Board[destination.X, destination.Y] is Pieces.Rook)
                    {
                        if (destination == new Point(0, 0))
                        {
                            WhiteMayCastleQueenside = false;
                        }
                        else if (destination == new Point(7, 0))
                        {
                            WhiteMayCastleKingside = false;
                        }
                        else if (destination == new Point(0, 7))
                        {
                            BlackMayCastleQueenside = false;
                        }
                        else if (destination == new Point(7, 7))
                        {
                            BlackMayCastleKingside = false;
                        }
                    }
                    CapturedPieces.Add(Board[destination.X, destination.Y]!);
                    StaleMoveCounter = 0;
                }

                EnPassantSquare = null;
                if (piece is Pieces.Pawn)
                {
                    StaleMoveCounter = 0;
                    if (Math.Abs(destination.Y - source.Y) > 1)
                    {
                        EnPassantSquare = new Point(source.X, source.Y + (piece.IsWhite ? 1 : -1));
                    }
                    if (destination.Y == (piece.IsWhite ? 7 : 0))
                    {
                        // TODO: Promotion into any piece, not just Queen
                        piece = new Pieces.Queen(piece.Position, piece.IsWhite);
                        Board[source.X, source.Y] = piece;
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
