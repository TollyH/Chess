using System.Drawing;

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

        private const string files = "abcdefgh";
        private const string ranks = "12345678";
        public static string ToChessCoordinate(this Point point)
        {
            return $"{files[point.X]}{ranks[point.Y]}";
        }

        public static Point FromChessCoordinate(this string coordinate)
        {
            return new Point(files.IndexOf(coordinate[0]), ranks.IndexOf(coordinate[1]));
        }
    }
}
