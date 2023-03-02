using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Chess
{
    /// <summary>
    /// Interaction logic for CustomGame.xaml
    /// </summary>
    public partial class CustomGame : Window
    {
        public Pieces.Piece?[,] Board { get; private set; }
        public System.Drawing.Point? EnPassantSquare { get; private set; }

        public ChessGame? GeneratedGame { get; private set; }
        public bool WhiteIsComputer { get; private set; }
        public bool BlackIsComputer { get; private set; }

        private Pieces.King? whiteKing = null;
        private Pieces.King? blackKing = null;

        private double tileWidth;
        private double tileHeight;

        public CustomGame()
        {
            Board = new Pieces.Piece?[8, 8];
            EnPassantSquare = null;
            GeneratedGame = null;

            InitializeComponent();
        }

        public void UpdateBoard()
        {
            tileWidth = chessGameCanvas.ActualWidth / Board.GetLength(0);
            tileHeight = chessGameCanvas.ActualHeight / Board.GetLength(1);

            chessGameCanvas.Children.Clear();

            if (EnPassantSquare is not null)
            {
                Rectangle enPassantHighlight = new()
                {
                    Width = tileWidth,
                    Height = tileHeight,
                    Fill = Brushes.OrangeRed
                };
                _ = chessGameCanvas.Children.Add(enPassantHighlight);
                Canvas.SetBottom(enPassantHighlight, EnPassantSquare.Value.Y * tileHeight);
                Canvas.SetLeft(enPassantHighlight, EnPassantSquare.Value.X * tileWidth);
            }

            for (int x = 0; x < Board.GetLength(0); x++)
            {
                for (int y = 0; y < Board.GetLength(1); y++)
                {
                    Pieces.Piece? piece = Board[x, y];
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

            startButton.IsEnabled = whiteKing is not null && blackKing is not null;

            if (whiteKing is null || whiteKing.Position != new System.Drawing.Point(4, 0))
            {
                castleWhiteKingside.IsChecked = false;
                castleWhiteKingside.IsEnabled = false;
                castleWhiteQueenside.IsChecked = false;
                castleWhiteQueenside.IsEnabled = false;
            }
            else
            {
                castleWhiteKingside.IsEnabled = true;
                castleWhiteQueenside.IsEnabled = true;
            }

            if (Board[0, 0] is not Pieces.Rook || !Board[0, 0]!.IsWhite)
            {
                castleWhiteQueenside.IsChecked = false;
                castleWhiteQueenside.IsEnabled = false;
            }
            if (Board[7, 0] is not Pieces.Rook || !Board[7, 0]!.IsWhite)
            {
                castleWhiteKingside.IsChecked = false;
                castleWhiteKingside.IsEnabled = false;
            }

            if (blackKing is null || blackKing.Position != new System.Drawing.Point(4, 7))
            {
                castleBlackKingside.IsChecked = false;
                castleBlackKingside.IsEnabled = false;
                castleBlackQueenside.IsChecked = false;
                castleBlackQueenside.IsEnabled = false;
            }
            else
            {
                castleBlackKingside.IsEnabled = true;
                castleBlackQueenside.IsEnabled = true;
            }

            if (Board[0, 7] is not Pieces.Rook || Board[0, 7]!.IsWhite)
            {
                castleBlackQueenside.IsChecked = false;
                castleBlackQueenside.IsEnabled = false;
            }
            if (Board[7, 7] is not Pieces.Rook || Board[7, 7]!.IsWhite)
            {
                castleBlackKingside.IsChecked = false;
                castleBlackKingside.IsEnabled = false;
            }
        }

        private void clearEnPassantButton_Click(object sender, RoutedEventArgs e)
        {
            EnPassantSquare = null;
            UpdateBoard();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            WhiteIsComputer = computerSelectWhite.IsChecked ?? false;
            BlackIsComputer = computerSelectBlack.IsChecked ?? false;
            bool currentTurnWhite = turnSelectWhite.IsChecked ?? false;
            // For the PGN standard, if black moves first then a single move "..." is added to the start of the move text list
            GeneratedGame = new ChessGame(Board, currentTurnWhite,
                ChessGame.EndingStates.Contains(BoardAnalysis.DetermineGameState(Board, currentTurnWhite)),
                new(), currentTurnWhite ? new() : new() { "..." }, new(), EnPassantSquare, castleWhiteKingside.IsChecked ?? false,
                castleWhiteQueenside.IsChecked ?? false, castleBlackKingside.IsChecked ?? false,
                castleBlackQueenside.IsChecked ?? false, 0, new(), null, null);
            Close();
        }

        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            importOverlay.Visibility = Visibility.Visible;
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point mousePoint = Mouse.GetPosition(chessGameCanvas);
            if (mousePoint.X < 0 || mousePoint.Y < 0
                || mousePoint.X > chessGameCanvas.ActualWidth || mousePoint.Y > chessGameCanvas.ActualHeight)
            {
                return;
            }

            // Canvas coordinates are relative to top-left, whereas chess' are from bottom-left, so y is inverted
            System.Drawing.Point coord = new((int)(mousePoint.X / tileWidth),
                (int)((chessGameCanvas.ActualHeight - mousePoint.Y) / tileHeight));
            if (coord.X < 0 || coord.Y < 0 || coord.X >= Board.GetLength(0) || coord.Y >= Board.GetLength(1))
            {
                return;
            }

            if (e.ChangedButton == MouseButton.Left && (enPassantSelect.IsChecked ?? false))
            {
                EnPassantSquare = coord;
            }
            else
            {
                if (Board[coord.X, coord.Y] is null)
                {
                    bool white = e.ChangedButton == MouseButton.Left;
                    if (pieceSelectKing.IsChecked ?? false)
                    {
                        // Only allow one king of each colour
                        if (white && whiteKing is null)
                        {
                            whiteKing = new Pieces.King(coord, true);
                            Board[coord.X, coord.Y] = whiteKing;
                        }
                        else if (!white && blackKing is null)
                        {
                            blackKing = new Pieces.King(coord, false);
                            Board[coord.X, coord.Y] = blackKing;
                        }
                    }
                    else if (pieceSelectQueen.IsChecked ?? false)
                    {
                        Board[coord.X, coord.Y] = new Pieces.Queen(coord, white);
                    }
                    else if (pieceSelectRook.IsChecked ?? false)
                    {
                        Board[coord.X, coord.Y] = new Pieces.Rook(coord, white);
                    }
                    else if (pieceSelectBishop.IsChecked ?? false)
                    {
                        Board[coord.X, coord.Y] = new Pieces.Bishop(coord, white);
                    }
                    else if (pieceSelectKnight.IsChecked ?? false)
                    {
                        Board[coord.X, coord.Y] = new Pieces.Knight(coord, white);
                    }
                    else if (pieceSelectPawn.IsChecked ?? false)
                    {
                        Board[coord.X, coord.Y] = new Pieces.Pawn(coord, white);
                    }
                }
                else
                {
                    if (Board[coord.X, coord.Y] is Pieces.King king)
                    {
                        if (king.IsWhite)
                        {
                            whiteKing = null;
                        }
                        else
                        {
                            blackKing = null;
                        }
                    }
                    Board[coord.X, coord.Y] = null;
                }
            }

            UpdateBoard();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateBoard();
        }

        private void submitFenButton_Click(object sender, RoutedEventArgs e)
        {
            WhiteIsComputer = computerSelectWhite.IsChecked ?? false;
            BlackIsComputer = computerSelectBlack.IsChecked ?? false;
            try
            {
                GeneratedGame = ChessGame.FromForsythEdwards(fenInput.Text);
                Close();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message, "Forsyth–Edwards Notation Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cancelFenButton_Click(object sender, RoutedEventArgs e)
        {
            importOverlay.Visibility = Visibility.Hidden;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBoard();
        }
    }
}
