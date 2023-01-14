using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Chess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChessGame game = new();

        private Pieces.Piece? grabbedPiece = null;
        /// <summary>
        /// <see langword="true"/> if the player has selected a piece but isn't dragging it, <see langword="false"/> otherwise
        /// </summary>
        private bool highlightGrabbedMoves = false;

        private readonly Dictionary<Pieces.Piece, Viewbox> pieceViews = new();

        double tileWidth;
        double tileHeight;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void UpdateGameDisplay()
        {
            chessGameCanvas.Children.Clear();
            pieceViews.Clear();

            tileWidth = chessGameCanvas.ActualWidth / game.Board.GetLength(0);
            tileHeight = chessGameCanvas.ActualHeight / game.Board.GetLength(1);

            if (grabbedPiece is not null && highlightGrabbedMoves)
            {
                foreach (System.Drawing.Point validMove in grabbedPiece.GetValidMoves(game.Board, true))
                {
                    Rectangle newRect = new()
                    {
                        Width = tileWidth,
                        Height = tileHeight,
                        Fill = game.Board[validMove.X, validMove.Y] is null ? Brushes.Yellow : Brushes.Red
                    };
                    _ = chessGameCanvas.Children.Add(newRect);
                    Canvas.SetBottom(newRect, validMove.Y * tileHeight);
                    Canvas.SetLeft(newRect, validMove.X * tileWidth);
                }
            }

            for (int x = 0; x < game.Board.GetLength(0); x++)
            {
                for (int y = 0; y < game.Board.GetLength(1); y++)
                {
                    Pieces.Piece? piece = game.Board[x, y];
                    if (piece is not null)
                    {
                        Viewbox newPiece = new()
                        {
                            Child = new TextBlock() 
                            {
                                Text = piece.SymbolSpecial.ToString(),
                                FontFamily = new("Segoe UI Symbol")
                            },
                            Width = tileWidth,
                            Height = tileHeight
                        };
                        pieceViews[piece] = newPiece;
                        _ = chessGameCanvas.Children.Add(newPiece);
                        Canvas.SetBottom(newPiece, y * tileHeight);
                        Canvas.SetLeft(newPiece, x * tileWidth);
                    }
                }
            }
        }

        private void UpdateCursor()
        {
            if (grabbedPiece is not null && !highlightGrabbedMoves)
            {
                Mouse.OverrideCursor = Cursors.ScrollAll;
                return;
            }

            Pieces.Piece? checkPiece = GetPieceAtCanvasPoint(Mouse.GetPosition(chessGameCanvas));
            if (checkPiece is not null && checkPiece.IsWhite == game.CurrentTurnWhite)
            {
                Mouse.OverrideCursor = Cursors.Hand;
                return;
            }

            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private System.Drawing.Point GetCoordFromCanvasPoint(Point position)
        {
            // Canvas coordinates are relative to top-left, whereas chess' are from bottom-left, so y is inverted
            return new System.Drawing.Point((int)(position.X / tileWidth), (int)((chessGameCanvas.ActualHeight - position.Y) / tileHeight));
        }

        private Pieces.Piece? GetPieceAtCanvasPoint(Point position)
        {
            System.Drawing.Point coord = GetCoordFromCanvasPoint(position);
            return coord.X < 0 || coord.Y < 0 || coord.X >= game.Board.GetLength(0) || coord.Y >= game.Board.GetLength(1)
                ? null
                : game.Board[coord.X, coord.Y];
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGameDisplay();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGameDisplay();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (grabbedPiece is not null && !highlightGrabbedMoves)
            {
                Canvas.SetBottom(pieceViews[grabbedPiece], chessGameCanvas.ActualHeight - Mouse.GetPosition(chessGameCanvas).Y - (tileHeight / 2));
                Canvas.SetLeft(pieceViews[grabbedPiece], Mouse.GetPosition(chessGameCanvas).X - (tileWidth / 2));
            }
            UpdateCursor();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePos = Mouse.GetPosition(chessGameCanvas);

            // If a piece is selected, try to move it
            if (grabbedPiece is not null && highlightGrabbedMoves)
            {
                System.Drawing.Point destination = GetCoordFromCanvasPoint(mousePos);
                bool success = game.MovePiece(grabbedPiece.Position, destination);
                if (success)
                {
                    highlightGrabbedMoves = false;
                    grabbedPiece = null;
                    UpdateCursor();
                    UpdateGameDisplay();
                    return;
                }
            }

            highlightGrabbedMoves = false;
            Pieces.Piece? toCheck = GetPieceAtCanvasPoint(mousePos);
            grabbedPiece = toCheck is null || toCheck.IsWhite == game.CurrentTurnWhite ? toCheck : null;
            UpdateGameDisplay();
            UpdateCursor();
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (grabbedPiece is not null)
            {
                System.Drawing.Point destination = GetCoordFromCanvasPoint(Mouse.GetPosition(chessGameCanvas));
                if (destination == grabbedPiece.Position)
                {
                    highlightGrabbedMoves = true;
                    UpdateCursor();
                    UpdateGameDisplay();
                    return;
                }
                bool success = game.MovePiece(grabbedPiece.Position, destination);
                if (!success)
                {
                    highlightGrabbedMoves = true;
                }
                else
                {
                    grabbedPiece = null;
                    highlightGrabbedMoves = false;
                }
            }
            UpdateCursor();
            UpdateGameDisplay();
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            highlightGrabbedMoves = true;
            UpdateGameDisplay();
        }
    }
}
