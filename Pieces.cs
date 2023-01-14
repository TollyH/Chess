using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;

namespace Chess.Pieces
{
    public abstract class Piece
    {
        public abstract string Name { get; }
        public abstract char SymbolLetter { get; }
        public abstract char SymbolSpecial { get; }
        public abstract double Value { get; }
        public abstract bool IsWhite { get; }
        public abstract Point Position { get; protected set; }
        public abstract King ParentKing { get; }

        /// <summary>
        /// Get a set of all moves that this piece can perform on the given board
        /// </summary>
        /// <param name="enforceCheckLegality">Whether or not moves that would put own king into check are discounted</param>
        public abstract HashSet<Point> GetValidMoves(Piece?[,] board, bool enforceCheckLegality);

        /// <summary>
        /// Move this piece to a square on the board
        /// </summary>
        /// <returns><see langword="true"/> if the move was valid and executed, <see langword="false"/> otherwise</returns>
        /// <remarks>This method will ensure that the move is valid</remarks>
        public bool Move(Piece?[,] board, Point target)
        {
            if (GetValidMoves(board, true).Contains(target))
            {
                Position = target;
                return true;
            }
            return false;
        }
    }

    public class King : Piece
    {
        public override string Name => "King";
        public override char SymbolLetter => 'K';
        public override char SymbolSpecial { get; }
        public override double Value => double.PositiveInfinity;
        public override bool IsWhite { get; }
        public override Point Position { get; protected set; }
        public override King ParentKing => this;

        public King(Point position, bool isWhite)
        {
            Position = position;
            IsWhite = isWhite;
            SymbolSpecial = isWhite ? '♔' : '♚';
        }

        public override HashSet<Point> GetValidMoves(Piece?[,] board, bool enforceCheckLegality)
        {
            HashSet<Point> moves = new();
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dy != 0 || dx != 0)
                    {
                        Point newPos = new(Position.X + dx, Position.Y + dy);
                        if (newPos.X >= 0 && newPos.Y >= 0 && newPos.X < board.GetLength(0) && newPos.Y < board.GetLength(1)
                            && (board[newPos.X, newPos.Y] is null || board[newPos.X, newPos.Y]!.IsWhite != IsWhite))
                        {
                            _ = moves.Add(newPos);
                        }
                    }
                }
            }
            if (enforceCheckLegality)
            {
                _ = moves.RemoveWhere(m => BoardAnalysis.IsSquareOpponentReachable(board.AfterMove(Position, m), m, IsWhite));
            }
            return moves;
        }
    }

    public class Queen : Piece
    {
        public override string Name => "Queen";
        public override char SymbolLetter => 'Q';
        public override char SymbolSpecial { get; }
        public override double Value => 9.5;
        public override bool IsWhite { get; }
        public override Point Position { get; protected set; }
        public override King ParentKing { get; }

        public Queen(Point position, bool isWhite, King parentKing)
        {
            Position = position;
            IsWhite = isWhite;
            SymbolSpecial = isWhite ? '♕' : '♛';
            ParentKing = parentKing;
        }

        public override HashSet<Point> GetValidMoves(Piece?[,] board, bool enforceCheckLegality)
        {
            HashSet<Point> moves = new();
            // Right
            for (int dx = Position.X + 1; dx < board.GetLength(0); dx++)
            {
                Point newPos = new(dx, Position.Y);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            // Left
            for (int dx = Position.X - 1; dx >= 0; dx--)
            {
                Point newPos = new(dx, Position.Y);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            // Up
            for (int dy = Position.Y + 1; dy < board.GetLength(1); dy++)
            {
                Point newPos = new(Position.X, dy);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            // Down
            for (int dy = Position.Y - 1; dy >= 0; dy--)
            {
                Point newPos = new(Position.X, dy);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            // Diagonal Up Right
            for (int dif = 1; Position.X + dif < board.GetLength(0) && Position.Y + dif < board.GetLength(1); dif++)
            {
                Point newPos = new(Position.X + dif, Position.Y + dif);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            // Diagonal Up Left
            for (int dif = 1; Position.X - dif >= 0 && Position.Y + dif < board.GetLength(1); dif++)
            {
                Point newPos = new(Position.X - dif, Position.Y + dif);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            // Diagonal Down Left
            for (int dif = 1; Position.X - dif >= 0 && Position.Y - dif >= 0; dif++)
            {
                Point newPos = new(Position.X - dif, Position.Y - dif);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            // Diagonal Down Right
            for (int dif = 1; Position.X + dif < board.GetLength(0) && Position.Y - dif >= 0; dif++)
            {
                Point newPos = new(Position.X + dif, Position.Y - dif);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            if (enforceCheckLegality)
            {
                _ = moves.RemoveWhere(m => BoardAnalysis.IsSquareOpponentReachable(board.AfterMove(Position, m), ParentKing.Position, IsWhite));
            }
            return moves;
        }
    }

    public class Rook : Piece
    {
        public override string Name => "Rook";
        public override char SymbolLetter => 'R';
        public override char SymbolSpecial { get; }
        public override double Value => 5.63;
        public override bool IsWhite { get; }
        public override Point Position { get; protected set; }
        public override King ParentKing { get; }

        public Rook(Point position, bool isWhite, King parentKing)
        {
            Position = position;
            IsWhite = isWhite;
            SymbolSpecial = isWhite ? '♖' : '♜';
            ParentKing = parentKing;
        }

        public override HashSet<Point> GetValidMoves(Piece?[,] board, bool enforceCheckLegality)
        {
            HashSet<Point> moves = new();
            // Right
            for (int dx = Position.X + 1; dx < board.GetLength(0); dx++)
            {
                Point newPos = new(dx, Position.Y);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            // Left
            for (int dx = Position.X - 1; dx >= 0; dx--)
            {
                Point newPos = new(dx, Position.Y);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            // Up
            for (int dy = Position.Y + 1; dy < board.GetLength(1); dy++)
            {
                Point newPos = new(Position.X, dy);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            // Down
            for (int dy = Position.Y - 1; dy >= 0; dy--)
            {
                Point newPos = new(Position.X, dy);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            if (enforceCheckLegality)
            {
                _ = moves.RemoveWhere(m => BoardAnalysis.IsSquareOpponentReachable(board.AfterMove(Position, m), ParentKing.Position, IsWhite));
            }
            return moves;
        }
    }

    public class Bishop : Piece
    {
        public override string Name => "Bishop";
        public override char SymbolLetter => 'B';
        public override char SymbolSpecial { get; }
        public override double Value => 3.33;
        public override bool IsWhite { get; }
        public override Point Position { get; protected set; }
        public override King ParentKing { get; }

        public Bishop(Point position, bool isWhite, King parentKing)
        {
            Position = position;
            IsWhite = isWhite;
            SymbolSpecial = isWhite ? '♗' : '♝';
            ParentKing = parentKing;
        }

        public override HashSet<Point> GetValidMoves(Piece?[,] board, bool enforceCheckLegality)
        {
            HashSet<Point> moves = new();
            // Diagonal Up Right
            for (int dif = 1; Position.X + dif < board.GetLength(0) && Position.Y + dif < board.GetLength(1); dif++)
            {
                Point newPos = new(Position.X + dif, Position.Y + dif);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            // Diagonal Up Left
            for (int dif = 1; Position.X - dif >= 0 && Position.Y + dif < board.GetLength(1); dif++)
            {
                Point newPos = new(Position.X - dif, Position.Y + dif);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            // Diagonal Down Left
            for (int dif = 1; Position.X - dif >= 0 && Position.Y - dif >= 0; dif++)
            {
                Point newPos = new(Position.X - dif, Position.Y - dif);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            // Diagonal Down Right
            for (int dif = 1; Position.X + dif < board.GetLength(0) && Position.Y - dif >= 0; dif++)
            {
                Point newPos = new(Position.X + dif, Position.Y - dif);
                if (board[newPos.X, newPos.Y] is null)
                {
                    _ = moves.Add(newPos);
                }
                else
                {
                    if (board[newPos.X, newPos.Y]!.IsWhite != IsWhite)
                    {
                        _ = moves.Add(newPos);
                    }
                    break;
                }
            }
            if (enforceCheckLegality)
            {
                _ = moves.RemoveWhere(m => BoardAnalysis.IsSquareOpponentReachable(board.AfterMove(Position, m), ParentKing.Position, IsWhite));
            }
            return moves;
        }
    }

    public class Knight : Piece
    {
        public static readonly ImmutableHashSet<Point> Moves = new HashSet<Point>()
        {
            new(1, 2), new(1, -2), new(-1, 2), new(-1, -2), new(2, 1), new(2, -1), new(-2, 1), new(-2, -1)
        }.ToImmutableHashSet();

        public override string Name => "Knight";
        public override char SymbolLetter => 'N';
        public override char SymbolSpecial { get; }
        public override double Value => 3.05;
        public override bool IsWhite { get; }
        public override Point Position { get; protected set; }
        public override King ParentKing { get; }

        public Knight(Point position, bool isWhite, King parentKing)
        {
            Position = position;
            IsWhite = isWhite;
            SymbolSpecial = isWhite ? '♘' : '♞';
            ParentKing = parentKing;
        }

        public override HashSet<Point> GetValidMoves(Piece?[,] board, bool enforceCheckLegality)
        {
            HashSet<Point> vectors = new(Moves);
            HashSet<Point> validMoves = new();
            foreach (Point newMove in vectors)
            {
                Point newPos = new(Position.X + newMove.X, Position.Y + newMove.Y);
                if (newPos.X >= 0 && newPos.Y >= 0 && newPos.X < board.GetLength(0) && newPos.Y < board.GetLength(1)
                    && (board[newPos.X, newPos.Y] is null || board[newPos.X, newPos.Y]!.IsWhite != IsWhite))
                {
                    _ = validMoves.Add(newPos);
                }
            }
            if (enforceCheckLegality)
            {
                _ = validMoves.RemoveWhere(m => BoardAnalysis.IsSquareOpponentReachable(board.AfterMove(Position, m), ParentKing.Position, IsWhite));
            }
            return validMoves;
        }
    }

    public class Pawn : Piece
    {
        public override string Name => "Pawn";
        public override char SymbolLetter => 'P';
        public override char SymbolSpecial { get; }
        public override double Value => 1;
        public override bool IsWhite { get; }
        public override Point Position { get; protected set; }
        public override King ParentKing { get; }
        public bool LastMoveWasDouble { get; set; }

        public Pawn(Point position, bool isWhite, King parentKing)
        {
            Position = position;
            IsWhite = isWhite;
            SymbolSpecial = isWhite ? '♙' : '♟';
            LastMoveWasDouble = false;
            ParentKing = parentKing;
        }

        public override HashSet<Point> GetValidMoves(Piece?[,] board, bool enforceCheckLegality)
        {
            HashSet<Point> moves = new();
            int checkY = Position.Y + (IsWhite ? 1 : -1);
            int doubleCheckY = Position.Y + (IsWhite ? 2 : -2);
            bool hasMoved = Position.Y != (IsWhite ? 1 : board.GetLength(1) - 2);
            if (board[Position.X, checkY] is null)
            {
                _ = moves.Add(new Point(Position.X, checkY));
            }
            // First move can optionally be two instead of one
            if (!hasMoved && board[Position.X, doubleCheckY] is null)
            {
                _ = moves.Add(new Point(Position.X, doubleCheckY));
            }
            // Taking to diagonal left
            if (Position.X > 0 && (
                (board[Position.X - 1, checkY] is not null && board[Position.X - 1, checkY]!.IsWhite != IsWhite)
                // En Passant
                || (board[Position.X - 1, Position.Y] is Pawn && board[Position.X - 1, Position.Y]!.IsWhite != IsWhite
                    && ((Pawn)board[Position.X - 1, Position.Y]!).LastMoveWasDouble)))
            {
                _ = moves.Add(new Point(Position.X - 1, checkY));
            }
            // Taking to diagonal right
            if (Position.X < board.GetLength(0) - 1 && (
                (board[Position.X + 1, checkY] is not null && board[Position.X + 1, checkY]!.IsWhite != IsWhite)
                // En Passant
                || (board[Position.X + 1, Position.Y] is Pawn && board[Position.X + 1, Position.Y]!.IsWhite != IsWhite
                    && ((Pawn)board[Position.X + 1, Position.Y]!).LastMoveWasDouble)))
            {
                _ = moves.Add(new Point(Position.X + 1, checkY));
            }
            if (enforceCheckLegality)
            {
                _ = moves.RemoveWhere(m => BoardAnalysis.IsSquareOpponentReachable(board.AfterMove(Position, m), ParentKing.Position, IsWhite));
            }
            return moves;
        }
    }
}
