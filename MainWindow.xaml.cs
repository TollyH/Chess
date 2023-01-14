using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChessGame game = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        public void UpdateGameDisplay()
        {
            chessGameCanvas.Children.Clear();

            double tileWidth = chessGameCanvas.ActualWidth / game.Board.GetLength(0);
            double tileHeight = chessGameCanvas.ActualHeight / game.Board.GetLength(1);

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
                        _ = chessGameCanvas.Children.Add(newPiece);
                        Canvas.SetBottom(newPiece, y * tileHeight);
                        Canvas.SetLeft(newPiece, x * tileWidth);
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGameDisplay();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGameDisplay();
        }
    }
}
