using System;
using System.Collections.Generic;
using System.Drawing;

namespace Chess.Pieces
{
    public interface IPiece
    {
        public string Name { get; }
        public char SymbolLetter { get; }
        public char SymbolSpecial { get; }
        public double Value { get; }
        public bool IsWhite { get; }
        public Point Position { get; }
        public King ParentKing { get; }

        /// <summary>
        /// Get a set of all moves that this piece can perform on the given board
        /// </summary>
        /// <param name="enforceCheckLegality">Whether or not moves that would put own king into check are discounted</param>
        public HashSet<Point> GetValidMoves(IPiece?[,] board, bool enforceCheckLegality);
        /// <summary>
        /// Move this piece to a square on the board
        /// </summary>
        /// <returns><see langword="true"/> if the move was valid and executed, <see langword="false"/> otherwise</returns>
        /// <remarks>This method will ensure that the move is valid</remarks>
        public bool Move(IPiece?[,] board, Point target);
    }

    public class King : IPiece
    {
        public string Name => "King";
        public char SymbolLetter => 'K';
        public char SymbolSpecial { get; private set; }
        public double Value => double.PositiveInfinity;
        public bool IsWhite { get; private set; }
        public Point Position { get; private set; }
        public King ParentKing => this;

        public King(Point position, bool isWhite)
        {
            Position = position;
            IsWhite = isWhite;
            SymbolSpecial = isWhite ? '♔' : '♚';
        }

        public HashSet<Point> GetValidMoves(IPiece?[,] board, bool enforceCheckLegality)
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
                _ = moves.RemoveWhere(m => BoardAnalysis.IsSquareOpponentReachable(board, m, IsWhite));
            }
            return moves;
        }

        public bool Move(IPiece?[,] board, Point target)
        {
            if (GetValidMoves(board, true).Contains(target))
            {
                Position = target;
                return true;
            }
            return false;
        }
    }

    public class Queen : IPiece
    {
        public string Name => "Queen";
        public char SymbolLetter => 'Q';
        public char SymbolSpecial { get; private set; }
        public double Value => 9.5;
        public bool IsWhite { get; private set; }
        public Point Position { get; private set; }
        public King ParentKing { get; private set; }

        public Queen(Point position, bool isWhite, King parentKing)
        {
            Position = position;
            IsWhite = isWhite;
            SymbolSpecial = isWhite ? '♕' : '♛';
            ParentKing = parentKing;
        }

        public HashSet<Point> GetValidMoves(IPiece?[,] board, bool enforceCheckLegality)
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

        public bool Move(IPiece?[,] board, Point target)
        {
            if (GetValidMoves(board, true).Contains(target))
            {
                Position = target;
                return true;
            }
            return false;
        }
    }

    public class Rook : IPiece
    {
        public string Name => "Rook";
        public char SymbolLetter => 'R';
        public char SymbolSpecial { get; private set; }
        public double Value => 5.63;
        public bool IsWhite { get; private set; }
        public Point Position { get; private set; }
        public King ParentKing { get; private set; }

        public Rook(Point position, bool isWhite, King parentKing)
        {
            Position = position;
            IsWhite = isWhite;
            SymbolSpecial = isWhite ? '♖' : '♜';
            ParentKing = parentKing;
        }

        public HashSet<Point> GetValidMoves(IPiece?[,] board, bool enforceCheckLegality)
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

        public bool Move(IPiece?[,] board, Point target)
        {
            if (GetValidMoves(board, true).Contains(target))
            {
                Position = target;
                return true;
            }
            return false;
        }
    }

    public class Bishop : IPiece
    {
        public string Name => "Bishop";
        public char SymbolLetter => 'B';
        public char SymbolSpecial { get; private set; }
        public double Value => 3.33;
        public bool IsWhite { get; private set; }
        public Point Position { get; private set; }
        public King ParentKing { get; private set; }

        public Bishop(Point position, bool isWhite, King parentKing)
        {
            Position = position;
            IsWhite = isWhite;
            SymbolSpecial = isWhite ? '♗' : '♝';
            ParentKing = parentKing;
        }

        public HashSet<Point> GetValidMoves(IPiece?[,] board, bool enforceCheckLegality)
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

        public bool Move(IPiece?[,] board, Point target)
        {
            if (GetValidMoves(board, true).Contains(target))
            {
                Position = target;
                return true;
            }
            return false;
        }
    }

    public class Knight : IPiece
    {
        public string Name => "Knight";
        public char SymbolLetter => 'N';
        public char SymbolSpecial { get; private set; }
        public double Value => 3.05;
        public bool IsWhite { get; private set; }
        public Point Position { get; private set; }
        public King ParentKing { get; private set; }

        public Knight(Point position, bool isWhite, King parentKing)
        {
            Position = position;
            IsWhite = isWhite;
            SymbolSpecial = isWhite ? '♘' : '♞';
            ParentKing = parentKing;
        }

        public HashSet<Point> GetValidMoves(IPiece?[,] board, bool enforceCheckLegality)
        {
            HashSet<Point> moves = new() { new(1, 2), new(1, -2), new(-1, 2), new(-1, -2), new(2, 1), new(2, -1), new(-2, 1), new(-2, -1) };
            foreach (Point newMove in moves)
            {
                Point newPos = new(Position.X + newMove.X, Position.Y + newMove.Y);
                if (newPos.X < 0 || newPos.Y < 0 || newPos.X >= board.GetLength(0) || newPos.Y >= board.GetLength(1)
                    || (board[newPos.X, newPos.Y] is not null && board[newPos.X, newPos.Y]!.IsWhite == IsWhite))
                {
                    _ = moves.Remove(newPos);
                }
            }
            if (enforceCheckLegality)
            {
                _ = moves.RemoveWhere(m => BoardAnalysis.IsSquareOpponentReachable(board.AfterMove(Position, m), ParentKing.Position, IsWhite));
            }
            return moves;
        }

        public bool Move(IPiece?[,] board, Point target)
        {
            if (GetValidMoves(board, true).Contains(target))
            {
                Position = target;
                return true;
            }
            return false;
        }
    }

    public class Pawn : IPiece
    {
        public string Name => "Pawn";
        public char SymbolLetter => 'P';
        public char SymbolSpecial { get; private set; }
        public double Value => 1;
        public bool IsWhite { get; private set; }
        public Point Position { get; private set; }
        public King ParentKing { get; private set; }
        public bool LastMoveWasDouble { get; private set; }

        public Pawn(Point position, bool isWhite, King parentKing)
        {
            Position = position;
            IsWhite = isWhite;
            SymbolSpecial = isWhite ? '♙' : '♟';
            LastMoveWasDouble = false;
            ParentKing = parentKing;
        }

        public HashSet<Point> GetValidMoves(IPiece?[,] board, bool enforceCheckLegality)
        {
            HashSet<Point> moves = new();
            int dy = IsWhite ? 1 : -1;
            bool hasMoved = Position.Y == (IsWhite ? 1 : board.GetLength(1) - 2);
            if (board[Position.X, Position.Y + dy] is null)
            {
                _ = moves.Add(new Point(Position.X, Position.Y + dy));
            }
            // First move can optionally be two instead of one
            if (!hasMoved && board[Position.X, Position.Y + (dy * 2)] is null)
            {
                _ = moves.Add(new Point(Position.X, Position.Y + (dy * 2)));
            }
            // Taking to diagonal left
            if (Position.X > 0 && (
                (board[Position.X - 1, Position.Y + dy] is not null && board[Position.X - 1, Position.Y + dy]!.IsWhite != IsWhite)
                // En Passant
                || (board[Position.X - 1, Position.Y + (dy * 2)] is not null && board[Position.X - 1, Position.Y + (dy * 2)]!.IsWhite != IsWhite
                    && board[Position.X - 1, Position.Y + (dy * 2)]!.GetType() == typeof(Pawn)
                    && ((Pawn)board[Position.X - 1, Position.Y + (dy * 2)]!).LastMoveWasDouble)))
            {
                _ = moves.Add(new Point(Position.X - 1, Position.Y + dy));
            }
            // Taking to diagonal right
            if (Position.X < board.GetLength(0) - 1 && (
                (board[Position.X + 1, Position.Y + dy] is not null && board[Position.X + 1, Position.Y + dy]!.IsWhite != IsWhite)
                // En Passant
                || (board[Position.X + 1, Position.Y + (dy * 2)] is not null && board[Position.X + 1, Position.Y + (dy * 2)]!.IsWhite != IsWhite
                    && board[Position.X + 1, Position.Y + (dy * 2)]!.GetType() == typeof(Pawn)
                    && ((Pawn)board[Position.X + 1, Position.Y + (dy * 2)]!).LastMoveWasDouble)))
            {
                _ = moves.Add(new Point(Position.X + 1, Position.Y + dy));
            }
            if (enforceCheckLegality)
            {
                _ = moves.RemoveWhere(m => BoardAnalysis.IsSquareOpponentReachable(board.AfterMove(Position, m), ParentKing.Position, IsWhite));
            }
            return moves;
        }

        public bool Move(IPiece?[,] board, Point target)
        {
            if (GetValidMoves(board, true).Contains(target))
            {
                LastMoveWasDouble = Math.Abs(target.Y - Position.Y) != 1;
                Position = target;
                return true;
            }
            return false;
        }
    }
}
