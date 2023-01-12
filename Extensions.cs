using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    public static class Extensions
    {
        /// <summary>
        /// Get a copy of the given board with the piece at oldPoint moved to newPoint.
        /// </summary>
        public static Pieces.IPiece?[,] AfterMove(this Pieces.IPiece?[,] board, Point oldPoint, Point newPoint)
        {
            Pieces.IPiece?[,] newBoard = (Pieces.IPiece?[,])board.Clone();
            newBoard[newPoint.X, newPoint.Y] = newBoard[oldPoint.X, oldPoint.Y];
            newBoard[oldPoint.X, oldPoint.Y] = null;
            return newBoard;
        }
    }
}
