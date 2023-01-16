using System;
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
                    Brush fillBrush;
                    if (game.Board[validMove.X, validMove.Y] is not null)
                    {
                        fillBrush = Brushes.Red;
                    }
                    else
                    {
                        fillBrush = Brushes.Yellow;
                    }

                    Rectangle newRect = new()
                    {
                        Width = tileWidth,
                        Height = tileHeight,
                        Fill = fillBrush
                    };
                    _ = chessGameCanvas.Children.Add(newRect);
                    Canvas.SetBottom(newRect, validMove.Y * tileHeight);
                    Canvas.SetLeft(newRect, validMove.X * tileWidth);
                }
            }

            if (grabbedPiece is Pieces.Pawn && game.EnPassantSquare is not null && highlightGrabbedMoves
                && Math.Abs(grabbedPiece.Position.X - game.EnPassantSquare.Value.X) == 1
                && Math.Abs(grabbedPiece.Position.Y - game.EnPassantSquare.Value.Y) == 1)
            {
                Rectangle enPassantHighlight = new()
                {
                    Width = tileWidth,
                    Height = tileHeight,
                    Fill = Brushes.OrangeRed
                };
                _ = chessGameCanvas.Children.Add(enPassantHighlight);
                Canvas.SetBottom(enPassantHighlight, game.EnPassantSquare.Value.Y * tileHeight);
                Canvas.SetLeft(enPassantHighlight, game.EnPassantSquare.Value.X * tileWidth);
            }

            GameState state = game.DetermineGameState();

            if (state is GameState.CheckMateWhite or GameState.CheckMateBlack)
            {
                System.Drawing.Point kingPosition = state == GameState.CheckMateWhite ? game.WhiteKing.Position : game.BlackKing.Position;
                Rectangle mateHighlight = new()
                {
                    Width = tileWidth,
                    Height = tileHeight,
                    Fill = Brushes.IndianRed
                };
                _ = chessGameCanvas.Children.Add(mateHighlight);
                Canvas.SetBottom(mateHighlight, kingPosition.Y * tileHeight);
                Canvas.SetLeft(mateHighlight, kingPosition.X * tileWidth);
            }

            if (grabbedPiece is Pieces.King && highlightGrabbedMoves)
            {
                int yPos = game.CurrentTurnWhite ? 0 : 7;
                if (game.IsCastlePossible(true))
                {
                    Rectangle castleHighlight = new()
                    {
                        Width = tileWidth,
                        Height = tileHeight,
                        Fill = Brushes.MediumPurple
                    };
                    _ = chessGameCanvas.Children.Add(castleHighlight);
                    Canvas.SetBottom(castleHighlight, yPos * tileHeight);
                    Canvas.SetLeft(castleHighlight, 6 * tileWidth);
                }
                if (game.IsCastlePossible(false))
                {
                    Rectangle castleHighlight = new()
                    {
                        Width = tileWidth,
                        Height = tileHeight,
                        Fill = Brushes.MediumPurple
                    };
                    _ = chessGameCanvas.Children.Add(castleHighlight);
                    Canvas.SetBottom(castleHighlight, yPos * tileHeight);
                    Canvas.SetLeft(castleHighlight, 2 * tileWidth);
                }
            }

            for (int x = 0; x < game.Board.GetLength(0); x++)
            {
                for (int y = 0; y < game.Board.GetLength(1); y++)
                {
                    Pieces.Piece? piece = game.Board[x, y];
                    if (piece is not null)
                    {
                        Brush foregroundBrush;
                        if (piece is Pieces.King && ((piece.IsWhite && state == GameState.CheckWhite) || (!piece.IsWhite && state == GameState.CheckBlack)))
                        {
                            foregroundBrush = Brushes.Red;
                        }
                        else if (highlightGrabbedMoves && piece == grabbedPiece)
                        {
                            foregroundBrush = Brushes.Blue;
                        }
                        else
                        {
                            foregroundBrush = Brushes.Black;
                        }

                        Viewbox newPiece = new()
                        {
                            Child = new TextBlock()
                            {
                                Text = piece.SymbolSpecial.ToString(),
                                FontFamily = new("Segoe UI Symbol"),
                                Foreground = foregroundBrush
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
            if (game.GameOver)
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                return;
            }
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

        /// <summary>
        /// If the game has ended, alert the user how it ended, otherwise do nothing
        /// </summary>
        private void PushEndgameMessage()
        {
            if (game.GameOver)
            {
                _ = MessageBox.Show(game.DetermineGameState() switch
                {
                    GameState.CheckMateWhite => "Black wins by checkmate!",
                    GameState.CheckMateBlack => "White wins by checkmate!",
                    GameState.DrawStalemate => "Game drawn due to stalemate",
                    GameState.DrawInsufficientMaterial => "Game drawn as neither side has sufficient material to mate",
                    GameState.DrawThreeFold => "Game drawn as the same position has occured three times",
                    GameState.DrawFiftyMove => "Game drawn as fifty moves have occured without a capture or a pawn movement",
                    _ => "Game over"
                }, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Perform a computer move if necessary
        /// </summary>
        private void CheckComputerMove()
        {
            // TODO: Currently black is always computer player, make that configurable
            // TODO: Stop blocking UI thread, player shouldn't be able to drag computer pieces with it unblocked

            if (!game.GameOver && !game.CurrentTurnWhite)
            {
                BoardAnalysis.PossibleMove bestMove = BoardAnalysis.EstimateBestPossibleMove(game, 4);
                _ = game.MovePiece(bestMove.Source, bestMove.Destination, true);
                UpdateGameDisplay();
                PushEndgameMessage();
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
            if (game.GameOver)
            {
                return;
            }
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
                    PushEndgameMessage();
                    CheckComputerMove();
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
            if (game.GameOver)
            {
                return;
            }
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
            PushEndgameMessage();
            CheckComputerMove();
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (grabbedPiece is not null)
            {
                highlightGrabbedMoves = true;
            }
            UpdateGameDisplay();
        }
    }
}
