using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChessGame game = new();

        private Pieces.Piece? grabbedPiece = null;

        private Dictionary<Pieces.Piece, Viewbox> pieceViews = new();

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
            if (grabbedPiece is not null)
            {
                Mouse.OverrideCursor = Cursors.ScrollAll;
            }
            else
            {
                Mouse.OverrideCursor = GetPieceAtCanvasPoint(Mouse.GetPosition(chessGameCanvas)) is null ? Cursors.Arrow : Cursors.Hand;
            }
        }

        private System.Drawing.Point GetCoordFromCanvasPoint(Point position)
        {
            // Canvas coordinates are relative to top-left, whereas chess' are from bottom-left, so y is inverted
            return new System.Drawing.Point((int)(position.X / tileWidth), (int)((chessGameCanvas.ActualHeight - position.Y) / tileHeight));
        }

        private Pieces.Piece? GetPieceAtCanvasPoint(Point position)
        {
            System.Drawing.Point coord = GetCoordFromCanvasPoint(position);
            if (coord.X < 0 || coord.Y < 0 || coord.X >= game.Board.GetLength(0) || coord.Y >= game.Board.GetLength(1))
            {
                return null;
            }
            return game.Board[coord.X, coord.Y];
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
            if (grabbedPiece is not null)
            {
                Canvas.SetBottom(pieceViews[grabbedPiece], (chessGameCanvas.ActualHeight - Mouse.GetPosition(chessGameCanvas).Y) - (tileHeight / 2));
                Canvas.SetLeft(pieceViews[grabbedPiece], (Mouse.GetPosition(chessGameCanvas).X) - (tileWidth / 2));
            }
            UpdateCursor();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePos = Mouse.GetPosition(chessGameCanvas);
            grabbedPiece = GetPieceAtCanvasPoint(mousePos);
            UpdateGameDisplay();
            UpdateCursor();
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            grabbedPiece = null;
            UpdateGameDisplay();
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            grabbedPiece = null;
            UpdateGameDisplay();
        }
    }
}
