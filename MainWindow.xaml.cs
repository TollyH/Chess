using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private bool whiteIsComputer = false;
        private bool blackIsComputer = true;

        private BoardAnalysis.PossibleMove? currentBestMove = null;
        private bool manuallyEvaluating = false;

        private readonly Dictionary<Pieces.Piece, Viewbox> pieceViews = new();

        double tileWidth;
        double tileHeight;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void UpdateGameDisplay()
        {
            if (game.AwaitingPromotionResponse)
            {
                return;
            }
            chessGameCanvas.Children.Clear();
            pieceViews.Clear();

            tileWidth = chessGameCanvas.ActualWidth / game.Board.GetLength(0);
            tileHeight = chessGameCanvas.ActualHeight / game.Board.GetLength(1);

            whiteCaptures.Content = game.CapturedPieces.Count(p => p.IsWhite);
            blackCaptures.Content = game.CapturedPieces.Count(p => !p.IsWhite);
            if (currentBestMove is null)
            {
                if (game.CurrentTurnWhite)
                {
                    whiteEvaluation.Content = "?";
                }
                else
                {
                    blackEvaluation.Content = "?";
                }
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

            if (game.Moves.Count > 0)
            {
                (System.Drawing.Point lastMoveSource, System.Drawing.Point lastMoveDestination) = game.Moves[^1];

                Rectangle sourceMoveHighlight = new()
                {
                    Width = tileWidth,
                    Height = tileHeight,
                    Fill = Brushes.CadetBlue
                };
                _ = chessGameCanvas.Children.Add(sourceMoveHighlight);
                Canvas.SetBottom(sourceMoveHighlight, lastMoveSource.Y * tileHeight);
                Canvas.SetLeft(sourceMoveHighlight, lastMoveSource.X * tileWidth);

                Rectangle destinationMoveHighlight = new()
                {
                    Width = tileWidth,
                    Height = tileHeight,
                    Fill = Brushes.Cyan
                };
                _ = chessGameCanvas.Children.Add(destinationMoveHighlight);
                Canvas.SetBottom(destinationMoveHighlight, lastMoveDestination.Y * tileHeight);
                Canvas.SetLeft(destinationMoveHighlight, lastMoveDestination.X * tileWidth);

            }

            if (currentBestMove is not null)
            {
                Rectangle bestMoveSrcHighlight = new()
                {
                    Width = tileWidth,
                    Height = tileHeight,
                    Fill = Brushes.LightGreen
                };
                _ = chessGameCanvas.Children.Add(bestMoveSrcHighlight);
                Canvas.SetBottom(bestMoveSrcHighlight, currentBestMove.Value.Source.Y * tileHeight);
                Canvas.SetLeft(bestMoveSrcHighlight, currentBestMove.Value.Source.X * tileWidth);

                Rectangle bestMoveDstHighlight = new()
                {
                    Width = tileWidth,
                    Height = tileHeight,
                    Fill = Brushes.Green
                };
                _ = chessGameCanvas.Children.Add(bestMoveDstHighlight);
                Canvas.SetBottom(bestMoveDstHighlight, currentBestMove.Value.Destination.Y * tileHeight);
                Canvas.SetLeft(bestMoveDstHighlight, currentBestMove.Value.Destination.X * tileWidth);
            }

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
                && grabbedPiece.Position.Y == (game.CurrentTurnWhite ? 4 : 3)
                && !BoardAnalysis.IsKingReachable(game.Board.AfterMove(
                        grabbedPiece.Position, game.EnPassantSquare.Value), game.CurrentTurnWhite))
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

            else if (grabbedPiece is Pieces.King && highlightGrabbedMoves)
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
            if (checkPiece is not null && ((checkPiece.IsWhite && game.CurrentTurnWhite && !whiteIsComputer)
                || (!checkPiece.IsWhite && !game.CurrentTurnWhite && !blackIsComputer)))
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

        private void UpdateEvaluationMeter(BoardAnalysis.PossibleMove bestMove, bool white)
        {
            Label toUpdate = white ? whiteEvaluation : blackEvaluation;
            if ((bestMove.WhiteMateLocated && !bestMove.BlackMateLocated)
                || bestMove.EvaluatedFutureValue == double.NegativeInfinity)
            {
                toUpdate.Content = $"-M{(int)Math.Ceiling(bestMove.DepthToWhiteMate / 2d)}";
            }
            else if ((bestMove.BlackMateLocated && !bestMove.WhiteMateLocated)
                || bestMove.EvaluatedFutureValue == double.PositiveInfinity)
            {
                toUpdate.Content = $"+M{(int)Math.Ceiling(bestMove.DepthToBlackMate / 2d)}";
            }
            else
            {
                toUpdate.Content = bestMove.EvaluatedFutureValue.ToString("+0.00;-0.00;0.00");
            }
        }

        /// <summary>
        /// Perform a computer move if necessary
        /// </summary>
        private async Task CheckComputerMove()
        {
            // TODO: Currently black is always computer player, make that configurable

            while (!game.GameOver && ((game.CurrentTurnWhite && whiteIsComputer) || (!game.CurrentTurnWhite && blackIsComputer)))
            {
                BoardAnalysis.PossibleMove bestMove = await BoardAnalysis.EstimateBestPossibleMove(game, 4);
                _ = game.MovePiece(bestMove.Source, bestMove.Destination, true);
                UpdateGameDisplay();
                // Turn has been inverted already but we have value for the now old turn
                UpdateEvaluationMeter(bestMove, !game.CurrentTurnWhite);
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
            if (position.X < 0 || position.Y < 0
                || position.X > chessGameCanvas.ActualWidth || position.Y > chessGameCanvas.ActualHeight)
            {
                return null;
            }
            System.Drawing.Point coord = GetCoordFromCanvasPoint(position);
            return coord.X < 0 || coord.Y < 0 || coord.X >= game.Board.GetLength(0) || coord.Y >= game.Board.GetLength(1)
                ? null
                : game.Board[coord.X, coord.Y];
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGameDisplay();
            await CheckComputerMove();
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

        private async void Window_MouseDown(object sender, MouseButtonEventArgs e)
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
                bool success = game.MovePiece(grabbedPiece.Position, destination, autoQueen: false);
                if (success)
                {
                    highlightGrabbedMoves = false;
                    grabbedPiece = null;
                    currentBestMove = null;
                    UpdateCursor();
                    UpdateGameDisplay();
                    PushEndgameMessage();
                    await CheckComputerMove();
                    return;
                }
            }

            highlightGrabbedMoves = false;
            Pieces.Piece? toCheck = GetPieceAtCanvasPoint(mousePos);
            if (toCheck is not null && !manuallyEvaluating)
            {
                if ((toCheck.IsWhite && game.CurrentTurnWhite && !whiteIsComputer)
                    || (!toCheck.IsWhite && !game.CurrentTurnWhite && !blackIsComputer))
                {
                    grabbedPiece = toCheck;
                }
                else
                {
                    grabbedPiece = null;
                }
            }
            else
            {
                grabbedPiece = null;
            }
            UpdateGameDisplay();
            UpdateCursor();
        }

        private async void Window_MouseUp(object sender, MouseButtonEventArgs e)
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
                bool success = game.MovePiece(grabbedPiece.Position, destination, autoQueen: false);
                if (success)
                {
                    grabbedPiece = null;
                    highlightGrabbedMoves = false;
                    currentBestMove = null;
                    UpdateCursor();
                    UpdateGameDisplay();
                    PushEndgameMessage();
                    await CheckComputerMove();
                    return;
                }
                else
                {
                    highlightGrabbedMoves = true;
                }
            }
            UpdateCursor();
            UpdateGameDisplay();
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (grabbedPiece is not null)
            {
                highlightGrabbedMoves = true;
            }
            UpdateGameDisplay();
        }

        private async void evaluation_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (currentBestMove is not null || (game.CurrentTurnWhite && whiteIsComputer)
                || (!game.CurrentTurnWhite && blackIsComputer))
            {
                return;
            }
            manuallyEvaluating = true;
            grabbedPiece = null;
            highlightGrabbedMoves = false;
            UpdateGameDisplay();
            UpdateCursor();
            BoardAnalysis.PossibleMove bestMove = await BoardAnalysis.EstimateBestPossibleMove(game, 4);
            UpdateEvaluationMeter(bestMove, game.CurrentTurnWhite);
            currentBestMove = bestMove;
            UpdateGameDisplay();
            manuallyEvaluating = false;
        }
    }
}
