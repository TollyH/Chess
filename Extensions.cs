using System.Drawing;
using System.Text;

namespace Chess
{
    public static class Extensions
    {
        /// <summary>
        /// Get a copy of the given board with the piece at <paramref name="oldPoint"/> moved to <paramref name="newPoint"/>.
        /// </summary>
        public static Pieces.Piece?[,] AfterMove(this Pieces.Piece?[,] board, Point oldPoint, Point newPoint)
        {
            Pieces.Piece?[,] newBoard = (Pieces.Piece?[,])board.Clone();
            newBoard[newPoint.X, newPoint.Y] = newBoard[oldPoint.X, oldPoint.Y];
            newBoard[oldPoint.X, oldPoint.Y] = null;
            return newBoard;
        }

        /// <summary>
        /// Get a string representation of the given board.
        /// </summary>
        /// <remarks>The string contains no whitespace and only stores information on piece type, colour, and location.</remarks>
        public static string ChessBoardToString(this Pieces.Piece?[,] board)
        {
            StringBuilder result = new(128);

            for (int y = 0; y < board.GetLength(1); y++)
            {
                for (int x = 0; x < board.GetLength(0); x++)
                {
                    Pieces.Piece? piece = board[x, y];
                    if (piece is null)
                    {
                        _ = result.Append("nn");
                    }
                    else
                    {
                        _ = result.Append(piece.SymbolLetter).Append(piece.IsWhite ? 'w' : 'b');
                    }
                }
            }

            return result.ToString();
        }
    }
}
