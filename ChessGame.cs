using System.Drawing;

namespace Chess
{
    public class ChessGame
    {
        public Pieces.Piece?[,] Board { get; private set; }
        public Pieces.King WhiteKing { get; private set; }
        public Pieces.King BlackKing { get; private set; }

        public ChessGame()
        {
            WhiteKing = new Pieces.King(new Point(4, 0), true);
            BlackKing = new Pieces.King(new Point(4, 7), false);

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
    }
}
