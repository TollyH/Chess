using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using Petzold.Media2D;

namespace Chess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private ChessGame game = new();
        private readonly Settings config;

        private Pieces.Piece? grabbedPiece = null;
        /// <summary>
        /// <see langword="true"/> if the player has selected a piece but isn't dragging it, <see langword="false"/> otherwise
        /// </summary>
        private bool highlightGrabbedMoves = false;

        private HashSet<System.Drawing.Point> squareHighlights = new();
        private HashSet<(System.Drawing.Point, System.Drawing.Point)> lineHighlights = new();
        private System.Drawing.Point? pointerPressedStartPoint = null;

        private bool whiteIsComputer = false;
        private bool blackIsComputer = false;

        private BoardAnalysis.PossibleMove? currentBestMove = null;
        private bool manuallyEvaluating = false;

        private bool updateDepthSliders = false;

        private readonly Dictionary<Pieces.Piece, Viewbox> pieceViews = new();

        private CancellationTokenSource cancelMoveComputation = new();

        /// <summary>
        /// <see langword="null"/> if an engine isn't found and built-in one should be used
        /// </summary>
        private string? enginePath = null;

        private double tileWidth;
        private double tileHeight;

        public MainWindow()
        {
            string jsonPath = System.IO.Path.Join(AppDomain.CurrentDomain.BaseDirectory, "chess-settings.json");
            config = File.Exists(jsonPath)
                ? JsonConvert.DeserializeObject<Settings>(File.ReadAllText(jsonPath)) ?? new Settings()
                : new Settings();

            if (config.ExternalEngineWhite || config.ExternalEngineBlack)
            {
                foreach (string filename in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.exe"))
                {
                    if (System.IO.Path.GetFileNameWithoutExtension(filename) != Process.GetCurrentProcess().ProcessName)
                    {
                        enginePath = filename;
                        break;
                    }
                }
            }

            InitializeComponent();

            rectSizeReference.Fill = new SolidColorBrush(config.DarkSquareColor);
            chessBoardBackground.Background = new SolidColorBrush(config.LightSquareColor);
            autoQueenItem.IsChecked = config.AutoQueen;
            moveListSymbolsItem.IsChecked = config.UseSymbolsOnMoveList;
            flipBoardItem.IsChecked = config.FlipBoard;
            updateEvalAfterBotItem.IsChecked = config.UpdateEvalAfterBot;
            externalEngineWhiteItem.IsChecked = config.ExternalEngineWhite;
            externalEngineBlackItem.IsChecked = config.ExternalEngineBlack;
            string externalDepthWhite = config.ExternalEngineWhiteDepth.ToString();
            string externalDepthBlack = config.ExternalEngineBlackDepth.ToString();
            foreach (MenuItem item in whiteExternalDepthItem.Items.OfType<MenuItem>())
            {
                if (string.Equals(externalDepthWhite, item.Tag as string, StringComparison.OrdinalIgnoreCase))
                {
                    item.IsChecked = true;
                    break;
                }
            }
            foreach (MenuItem item in blackExternalDepthItem.Items.OfType<MenuItem>())
            {
                if (string.Equals(externalDepthBlack, item.Tag as string, StringComparison.OrdinalIgnoreCase))
                {
                    item.IsChecked = true;
                    break;
                }
            }
            updateDepthSliders = true;
        }

        ~MainWindow()
        {
            Dispose();
        }

        public void Dispose()
        {
            cancelMoveComputation.Cancel();

            GC.SuppressFinalize(this);
        }

        public void UpdateGameDisplay()
        {
            chessGameCanvas.Children.Clear();
            pieceViews.Clear();

            bool boardFlipped = config.FlipBoard && ((!game.CurrentTurnWhite && !blackIsComputer) || (whiteIsComputer && !blackIsComputer));

            tileWidth = chessGameCanvas.Bounds.Width / game.Board.GetLength(0);
            tileHeight = chessGameCanvas.Bounds.Height / game.Board.GetLength(1);

            whiteCaptures.Content = 0;
            string tooltip = "";
            foreach (Pieces.Piece capturedPiece in game.CapturedPieces.Where(p => p.IsWhite))
            {
                whiteCaptures.Content = (int)whiteCaptures.Content + 1;
                tooltip += capturedPiece.Name + "\r\n";
            }
            ToolTip.SetTip(whiteCaptures, tooltip.TrimEnd());

            blackCaptures.Content = 0;
            foreach (Pieces.Piece capturedPiece in game.CapturedPieces.Where(p => !p.IsWhite))
            {
                blackCaptures.Content = (int)blackCaptures.Content + 1;
                tooltip += capturedPiece.Name + "\r\n";
            }
            ToolTip.SetTip(whiteCaptures, tooltip.TrimEnd());

            if (currentBestMove is null && !manuallyEvaluating)
            {
                if (game.CurrentTurnWhite && !whiteIsComputer)
                {
                    whiteEvaluation.Content = "?";
                }
                else if (!blackIsComputer)
                {
                    blackEvaluation.Content = "?";
                }
            }

            if (boardFlipped)
            {
                Grid.SetColumn(whiteEvaluationView, 2);
                Grid.SetRow(whiteEvaluationView, 0);
                Grid.SetColumn(blackEvaluationView, 0);
                Grid.SetRow(blackEvaluationView, 2);

                Grid.SetColumn(whiteCapturesView, 0);
                Grid.SetRow(whiteCapturesView, 0);
                Grid.SetColumn(blackCapturesView, 2);
                Grid.SetRow(blackCapturesView, 2);

                foreach (Control child in ranksLeft.Children)
                {
                    DockPanel.SetDock(child, Dock.Bottom);
                }
                foreach (Control child in ranksRight.Children)
                {
                    DockPanel.SetDock(child, Dock.Bottom);
                }
                foreach (Control child in filesTop.Children)
                {
                    DockPanel.SetDock(child, Dock.Right);
                }
                foreach (Control child in filesBottom.Children)
                {
                    DockPanel.SetDock(child, Dock.Right);
                }
            }
            else
            {
                Grid.SetColumn(whiteEvaluationView, 0);
                Grid.SetRow(whiteEvaluationView, 2);
                Grid.SetColumn(blackEvaluationView, 2);
                Grid.SetRow(blackEvaluationView, 0);

                Grid.SetColumn(whiteCapturesView, 2);
                Grid.SetRow(whiteCapturesView, 2);
                Grid.SetColumn(blackCapturesView, 0);
                Grid.SetRow(blackCapturesView, 0);

                foreach (Control child in ranksLeft.Children)
                {
                    DockPanel.SetDock(child, Dock.Top);
                }
                foreach (Control child in ranksRight.Children)
                {
                    DockPanel.SetDock(child, Dock.Top);
                }
                foreach (Control child in filesTop.Children)
                {
                    DockPanel.SetDock(child, Dock.Left);
                }
                foreach (Control child in filesBottom.Children)
                {
                    DockPanel.SetDock(child, Dock.Left);
                }
            }

            movesPanel.Children.Clear();
            for (int i = 0; i < game.MoveText.Count; i += 2)
            {
                string text = $"{(i / 2) + 1}. {game.MoveText[i]}";
                if (config.UseSymbolsOnMoveList)
                {
                    text = text.Replace('K', '♔').Replace('Q', '♕').Replace('R', '♖')
                        .Replace('B', '♗').Replace('N', '♘');
                }
                if (i + 1 < game.MoveText.Count)
                {
                    text += $" {game.MoveText[i + 1]}";
                    if (config.UseSymbolsOnMoveList)
                    {
                        text = text.Replace('K', '♚').Replace('Q', '♛').Replace('R', '♜')
                            .Replace('B', '♝').Replace('N', '♞');
                    }
                }
                movesPanel.Children.Add(new Label()
                {
                    Content = text,
                    FontSize = 18
                });
            }

            GameState state = game.DetermineGameState();

            int boardMaxY = game.Board.GetLength(1) - 1;
            int boardMaxX = game.Board.GetLength(0) - 1;

            if (state is GameState.CheckMateWhite or GameState.CheckMateBlack)
            {
                System.Drawing.Point kingPosition = state == GameState.CheckMateWhite ? game.WhiteKing.Position : game.BlackKing.Position;
                Rectangle mateHighlight = new()
                {
                    Width = tileWidth,
                    Height = tileHeight,
                    Fill = new SolidColorBrush(config.CheckMateHighlightColor)
                };
                chessGameCanvas.Children.Add(mateHighlight);
                Canvas.SetBottom(mateHighlight, (boardFlipped ? boardMaxY - kingPosition.Y : kingPosition.Y) * tileHeight);
                Canvas.SetLeft(mateHighlight, (boardFlipped ? boardMaxX - kingPosition.X : kingPosition.X) * tileWidth);
            }

            if (game.Moves.Count > 0)
            {
                (System.Drawing.Point lastMoveSource, System.Drawing.Point lastMoveDestination) = game.Moves[^1];

                Rectangle sourceMoveHighlight = new()
                {
                    Width = tileWidth,
                    Height = tileHeight,
                    Fill = new SolidColorBrush(config.LastMoveSourceColor)
                };
                chessGameCanvas.Children.Add(sourceMoveHighlight);
                Canvas.SetBottom(sourceMoveHighlight, (boardFlipped ? boardMaxY - lastMoveSource.Y : lastMoveSource.Y) * tileHeight);
                Canvas.SetLeft(sourceMoveHighlight, (boardFlipped ? boardMaxX - lastMoveSource.X : lastMoveSource.X) * tileWidth);

                Rectangle destinationMoveHighlight = new()
                {
                    Width = tileWidth,
                    Height = tileHeight,
                    Fill = new SolidColorBrush(config.LastMoveDestinationColor)
                };
                chessGameCanvas.Children.Add(destinationMoveHighlight);
                Canvas.SetBottom(destinationMoveHighlight, (boardFlipped ? boardMaxY - lastMoveDestination.Y : lastMoveDestination.Y) * tileHeight);
                Canvas.SetLeft(destinationMoveHighlight, (boardFlipped ? boardMaxX - lastMoveDestination.X : lastMoveDestination.X) * tileWidth);

            }

            if (currentBestMove is not null
                // Prevent cases where there are no valid moves highlighting (0, 0)
                && currentBestMove.Value.Source != currentBestMove.Value.Destination)
            {
                Rectangle bestMoveSrcHighlight = new()
                {
                    Width = tileWidth,
                    Height = tileHeight,
                    Fill = new SolidColorBrush(config.BestMoveSourceColor)
                };
                chessGameCanvas.Children.Add(bestMoveSrcHighlight);
                Canvas.SetBottom(bestMoveSrcHighlight,
                    (boardFlipped ? boardMaxY - currentBestMove.Value.Source.Y : currentBestMove.Value.Source.Y) * tileHeight);
                Canvas.SetLeft(bestMoveSrcHighlight,
                    (boardFlipped ? boardMaxX - currentBestMove.Value.Source.X : currentBestMove.Value.Source.X) * tileWidth);

                Rectangle bestMoveDstHighlight = new()
                {
                    Width = tileWidth,
                    Height = tileHeight,
                    Fill = new SolidColorBrush(config.BestMoveDestinationColor)
                };
                chessGameCanvas.Children.Add(bestMoveDstHighlight);
                Canvas.SetBottom(bestMoveDstHighlight,
                    (boardFlipped ? boardMaxY - currentBestMove.Value.Destination.Y : currentBestMove.Value.Destination.Y) * tileHeight);
                Canvas.SetLeft(bestMoveDstHighlight,
                    (boardFlipped ? boardMaxX - currentBestMove.Value.Destination.X : currentBestMove.Value.Destination.X) * tileWidth);
            }

            if (grabbedPiece is not null && highlightGrabbedMoves)
            {
                foreach (System.Drawing.Point validMove in grabbedPiece.GetValidMoves(game.Board, true))
                {
                    Brush fillBrush;
                    if (game.Board[validMove.X, validMove.Y] is not null)
                    {
                        fillBrush = new SolidColorBrush(config.AvailableCaptureColor);
                    }
                    else
                    {
                        fillBrush = new SolidColorBrush(config.AvailableMoveColor);
                    }

                    Rectangle newRect = new()
                    {
                        Width = tileWidth,
                        Height = tileHeight,
                        Fill = fillBrush
                    };
                    chessGameCanvas.Children.Add(newRect);
                    Canvas.SetBottom(newRect, (boardFlipped ? boardMaxY - validMove.Y : validMove.Y) * tileHeight);
                    Canvas.SetLeft(newRect, (boardFlipped ? boardMaxX - validMove.X : validMove.X) * tileWidth);
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
                    Fill = new SolidColorBrush(config.AvailableEnPassantColor)
                };
                chessGameCanvas.Children.Add(enPassantHighlight);
                Canvas.SetBottom(enPassantHighlight,
                    (boardFlipped ? boardMaxY - game.EnPassantSquare.Value.Y : game.EnPassantSquare.Value.Y) * tileHeight);
                Canvas.SetLeft(enPassantHighlight,
                    (boardFlipped ? boardMaxX - game.EnPassantSquare.Value.X : game.EnPassantSquare.Value.X) * tileWidth);
            }

            else if (grabbedPiece is Pieces.King && highlightGrabbedMoves)
            {
                int yPos = game.CurrentTurnWhite ? 0 : boardMaxY;
                if (game.IsCastlePossible(true))
                {
                    Rectangle castleHighlight = new()
                    {
                        Width = tileWidth,
                        Height = tileHeight,
                        Fill = new SolidColorBrush(config.AvailableCastleColor)
                    };
                    chessGameCanvas.Children.Add(castleHighlight);
                    Canvas.SetBottom(castleHighlight, (boardFlipped ? boardMaxY - yPos : yPos) * tileHeight);
                    Canvas.SetLeft(castleHighlight, (boardFlipped ? 1 : 6) * tileWidth);
                }
                if (game.IsCastlePossible(false))
                {
                    Rectangle castleHighlight = new()
                    {
                        Width = tileWidth,
                        Height = tileHeight,
                        Fill = new SolidColorBrush(config.AvailableCastleColor)
                    };
                    chessGameCanvas.Children.Add(castleHighlight);
                    Canvas.SetBottom(castleHighlight, (boardFlipped ? boardMaxY - yPos : yPos) * tileHeight);
                    Canvas.SetLeft(castleHighlight, (boardFlipped ? 5 : 2) * tileWidth);
                }
            }

            foreach (System.Drawing.Point square in squareHighlights)
            {
                Ellipse ellipse = new()
                {
                    Fill = new SolidColorBrush(config.SelectedPieceColor),
                    Opacity = 0.5,
                    Width = tileWidth * 0.8,
                    Height = tileHeight * 0.8
                };
                chessGameCanvas.Children.Add(ellipse);
                Canvas.SetBottom(ellipse, ((boardFlipped ? boardMaxY - square.Y : square.Y) * tileHeight) + (tileHeight * 0.1));
                Canvas.SetLeft(ellipse, (boardFlipped ? boardMaxX - square.X : square.X) * tileWidth + (tileWidth * 0.1));
            }

            foreach ((System.Drawing.Point lineStart, System.Drawing.Point lineEnd) in lineHighlights)
            {
                double arrowLength = Math.Min(tileWidth, tileHeight) / 4;
                ArrowLine line = new()
                {
                    Stroke = new SolidColorBrush(config.SelectedPieceColor),
                    Opacity = 0.5,
                    StrokeThickness = 10,
                    ArrowLength = arrowLength,
                    ArrowAngle = 0.785,
                    IsArrowClosed = true,
                    ArrowEnds = ArrowEnds.End,
                    X1 = (boardFlipped ? boardMaxX - lineStart.X : lineStart.X) * tileWidth + (tileWidth / 2),
                    X2 = (boardFlipped ? boardMaxX - lineEnd.X : lineEnd.X) * tileWidth + (tileWidth / 2),
                    Y1 = (boardFlipped ? lineStart.Y : boardMaxY - lineStart.Y) * tileHeight + (tileHeight / 2),
                    Y2 = (boardFlipped ? lineEnd.Y : boardMaxY - lineEnd.Y) * tileHeight + (tileHeight / 2)
                };
                chessGameCanvas.Children.Add(line);
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
                            foregroundBrush = new SolidColorBrush(config.CheckedKingColor);
                        }
                        else if (highlightGrabbedMoves && piece == grabbedPiece)
                        {
                            foregroundBrush = new SolidColorBrush(config.SelectedPieceColor);
                        }
                        else
                        {
                            foregroundBrush = new SolidColorBrush(config.DefaultPieceColor);
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
                        chessGameCanvas.Children.Add(newPiece);
                        Canvas.SetBottom(newPiece, (boardFlipped ? boardMaxY - y : y) * tileHeight);
                        Canvas.SetLeft(newPiece, (boardFlipped ? boardMaxX - x : x) * tileWidth);
                    }
                }
            }
        }

        private void UpdateCursor(Point mousePos)
        {
            if (game.GameOver)
            {
                Cursor = new Cursor(StandardCursorType.Arrow);
                return;
            }
            if (grabbedPiece is not null && !highlightGrabbedMoves)
            {
                Cursor = new Cursor(StandardCursorType.SizeAll);
                return;
            }
            Pieces.Piece? checkPiece = GetPieceAtCanvasPoint(mousePos);
            if (checkPiece is not null && ((checkPiece.IsWhite && game.CurrentTurnWhite && !whiteIsComputer)
                || (!checkPiece.IsWhite && !game.CurrentTurnWhite && !blackIsComputer)))
            {
                Cursor = new Cursor(StandardCursorType.Hand);
                return;
            }
            Cursor = new Cursor(StandardCursorType.Arrow);
        }

        /// <summary>
        /// If the game has ended, alert the user how it ended, otherwise do nothing
        /// </summary>
        private async Task PushEndgameMessage()
        {
            if (game.GameOver)
            {
                await new MessageBox(game.DetermineGameState() switch
                {
                    GameState.CheckMateWhite => "Black wins by checkmate!",
                    GameState.CheckMateBlack => "White wins by checkmate!",
                    GameState.DrawStalemate => "Game drawn due to stalemate",
                    GameState.DrawInsufficientMaterial => "Game drawn as neither side has sufficient material to mate",
                    GameState.DrawThreeFold => "Game drawn as the same position has occured three times",
                    GameState.DrawFiftyMove => "Game drawn as fifty moves have occured without a capture or a pawn movement",
                    _ => "Game over"
                }, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information).ShowDialog(this);
            }
        }

        private void UpdateEvaluationMeter(BoardAnalysis.PossibleMove? bestMove, bool white)
        {
            Label toUpdate = white ? whiteEvaluation : blackEvaluation;
            if (bestMove is null)
            {
                toUpdate.Content = "...";
                ToolTip.SetTip(toUpdate, null);
                return;
            }

            if ((bestMove.Value.WhiteMateLocated && !bestMove.Value.BlackMateLocated)
                || double.IsNegativeInfinity(bestMove.Value.EvaluatedFutureValue))
            {
                toUpdate.Content = $"-M{(int)Math.Ceiling(bestMove.Value.DepthToWhiteMate / 2d)}";
            }
            else if ((bestMove.Value.BlackMateLocated && !bestMove.Value.WhiteMateLocated)
                || double.IsPositiveInfinity(bestMove.Value.EvaluatedFutureValue))
            {
                toUpdate.Content = $"+M{(int)Math.Ceiling(bestMove.Value.DepthToBlackMate / 2d)}";
            }
            else
            {
                toUpdate.Content = bestMove.Value.EvaluatedFutureValue.ToString("+0.00;-0.00;0.00");
            }

            string convertedBestLine = "";
            ChessGame moveStringGenerator = game.Clone(false);
            foreach ((System.Drawing.Point source, System.Drawing.Point destination, Type promotionType) in bestMove.Value.BestLine)
            {
                _ = moveStringGenerator.MovePiece(source, destination, forceMove: true, promotionType: promotionType);
                convertedBestLine += " " + moveStringGenerator.MoveText[^1];
            }
            ToolTip.SetTip(toUpdate, convertedBestLine.Trim());
        }

        /// <summary>
        /// Get the best move according to either the built-in or external engine, depending on configuration
        /// </summary>
        private async Task<BoardAnalysis.PossibleMove> GetEngineMove(CancellationToken cancellationToken)
        {
            BoardAnalysis.PossibleMove? bestMove = null;
            if (enginePath is not null && ((config.ExternalEngineWhite && game.CurrentTurnWhite)
                || (config.ExternalEngineBlack && !game.CurrentTurnWhite)))
            {
                bestMove = await CommunicateUCI.GetBestMove(game, enginePath,
                    game.CurrentTurnWhite ? config.ExternalEngineWhiteDepth : config.ExternalEngineBlackDepth, cancellationToken);
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return default;
            }
            bestMove ??= await BoardAnalysis.EstimateBestPossibleMove(game, 4, true, cancellationToken);
            return bestMove.Value;
        }

        /// <summary>
        /// Perform a computer move if necessary
        /// </summary>
        private async Task CheckComputerMove()
        {
            while (!game.GameOver && ((game.CurrentTurnWhite && whiteIsComputer) || (!game.CurrentTurnWhite && blackIsComputer)))
            {
                CancellationToken cancellationToken = cancelMoveComputation.Token;
                if (config.UpdateEvalAfterBot)
                {
                    UpdateEvaluationMeter(null, game.CurrentTurnWhite);
                }
                BoardAnalysis.PossibleMove bestMove = await GetEngineMove(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                _ = game.MovePiece(bestMove.Source, bestMove.Destination, forceMove: true, promotionType: bestMove.PromotionType);
                UpdateGameDisplay();
                movesScroll.ScrollToEnd();
                if (config.UpdateEvalAfterBot)
                {
                    // Turn has been inverted already but we have value for the now old turn
                    UpdateEvaluationMeter(bestMove, !game.CurrentTurnWhite);
                }
                await PushEndgameMessage();
            }
        }

        private System.Drawing.Point GetCoordFromCanvasPoint(Point position)
        {
            bool boardFlipped = config.FlipBoard && ((!game.CurrentTurnWhite && !blackIsComputer) || (whiteIsComputer && !blackIsComputer));
            // Canvas coordinates are relative to top-left, whereas chess' are from bottom-left, so y is inverted
            return new System.Drawing.Point((int)((boardFlipped ? chessGameCanvas.Bounds.Width - position.X : position.X) / tileWidth),
                (int)((!boardFlipped ? chessGameCanvas.Bounds.Height - position.Y : position.Y) / tileHeight));
        }

        private Pieces.Piece? GetPieceAtCanvasPoint(Point position)
        {
            if (position.X < 0 || position.Y < 0
                || position.X > chessGameCanvas.Bounds.Width || position.Y > chessGameCanvas.Bounds.Height)
            {
                return null;
            }
            System.Drawing.Point coord = GetCoordFromCanvasPoint(position);
            return coord.X < 0 || coord.Y < 0 || coord.X >= game.Board.GetLength(0) || coord.Y >= game.Board.GetLength(1)
                ? null
                    : game.Board[coord.X, coord.Y];
        }

        private async Task NewGame()
        {
            cancelMoveComputation.Cancel();
            cancelMoveComputation = new CancellationTokenSource();
            game = new ChessGame();
            currentBestMove = null;
            manuallyEvaluating = false;
            grabbedPiece = null;
            highlightGrabbedMoves = false;
            whiteEvaluation.Content = "?";
            blackEvaluation.Content = "?";
            UpdateGameDisplay();
            UpdateCursor(new Point());
            await CheckComputerMove();
        }

        private async Task<Type?> GetPromotionType(System.Drawing.Point destination)
        {
            if (grabbedPiece is Pieces.Pawn &&
                destination.Y == (grabbedPiece.IsWhite ? game.Board.GetLength(1) - 1 : 0))
            {
                return config.AutoQueen
                    ? typeof(Pieces.Queen)
                    : await new PromotionPrompt(grabbedPiece.IsWhite).ShowDialog<Type>(this);
            }
            return null;
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

        private void Window_PointerMoved(object sender, PointerEventArgs e)
        {
            Point mousePos = e.GetPosition(chessGameCanvas);
            if (grabbedPiece is not null && !highlightGrabbedMoves)
            {
                Canvas.SetBottom(pieceViews[grabbedPiece], chessGameCanvas.Bounds.Height - mousePos.Y - (tileHeight / 2));
                Canvas.SetLeft(pieceViews[grabbedPiece], mousePos.X - (tileWidth / 2));
            }
            UpdateCursor(mousePos);
        }

        private async void Window_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            Point mousePos = e.GetPosition(chessGameCanvas);
            if (e.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
            {
                squareHighlights.Clear();
                lineHighlights.Clear();
                if (game.GameOver)
                {
                    return;
                }

                // If a piece is selected, try to move it
                if (grabbedPiece is not null && highlightGrabbedMoves)
                {
                    System.Drawing.Point destination = GetCoordFromCanvasPoint(mousePos);

                    bool success = game.MovePiece(grabbedPiece.Position, destination,
                        promotionType: await GetPromotionType(destination));
                    if (success)
                    {
                        highlightGrabbedMoves = false;
                        grabbedPiece = null;
                        currentBestMove = null;
                        UpdateCursor(mousePos);
                        UpdateGameDisplay();
                        movesScroll.ScrollToEnd();
                        await PushEndgameMessage();
                        await CheckComputerMove();
                        return;
                    }
                }

                highlightGrabbedMoves = false;
                Pieces.Piece? toCheck = GetPieceAtCanvasPoint(mousePos);
                if (toCheck is not null)
                {
                    if ((toCheck.IsWhite && game.CurrentTurnWhite && !whiteIsComputer)
                        || (!toCheck.IsWhite && !game.CurrentTurnWhite && !blackIsComputer))
                    {
                        grabbedPiece = toCheck;
                        manuallyEvaluating = false;
                        cancelMoveComputation.Cancel();
                        cancelMoveComputation = new CancellationTokenSource();
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
            }
            else
            {
                if (mousePos.X < 0 || mousePos.Y < 0
                || mousePos.X > chessGameCanvas.Bounds.Width || mousePos.Y > chessGameCanvas.Bounds.Height)
                {
                    return;
                }
                pointerPressedStartPoint = GetCoordFromCanvasPoint(mousePos);
            }
            UpdateGameDisplay();
            UpdateCursor(mousePos);
        }

        private async void Window_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            Point mousePos = e.GetPosition(chessGameCanvas);
            if (e.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
            {
                if (game.GameOver)
                {
                    return;
                }
                if (grabbedPiece is not null)
                {
                    System.Drawing.Point destination = GetCoordFromCanvasPoint(mousePos);
                    if (destination == grabbedPiece.Position)
                    {
                        highlightGrabbedMoves = true;
                        UpdateCursor(mousePos);
                        UpdateGameDisplay();
                        return;
                    }
                    bool success = game.MovePiece(grabbedPiece.Position, destination,
                        promotionType: await GetPromotionType(destination));
                    if (success)
                    {
                        grabbedPiece = null;
                        highlightGrabbedMoves = false;
                        currentBestMove = null;
                        UpdateCursor(mousePos);
                        UpdateGameDisplay();
                        movesScroll.ScrollToEnd();
                        await PushEndgameMessage();
                        await CheckComputerMove();
                        return;
                    }
                    else
                    {
                        highlightGrabbedMoves = true;
                    }
                }
            }
            else
            {
                if (mousePos.X < 0 || mousePos.Y < 0
                || mousePos.X > chessGameCanvas.Bounds.Width || mousePos.Y > chessGameCanvas.Bounds.Height)
                {
                    return;
                }
                System.Drawing.Point onSquare = GetCoordFromCanvasPoint(mousePos);
                if (pointerPressedStartPoint is null || pointerPressedStartPoint == onSquare)
                {
                    if (!squareHighlights.Add(onSquare))
                    {
                        _ = squareHighlights.Remove(onSquare);
                    }
                }
                else
                {
                    if (!lineHighlights.Add((pointerPressedStartPoint.Value, onSquare)))
                    {
                        _ = lineHighlights.Remove((pointerPressedStartPoint.Value, onSquare));
                    }
                }
            }
            UpdateCursor(mousePos);
            UpdateGameDisplay();
        }

        private void Window_PointerExited(object sender, PointerEventArgs e)
        {
            if (grabbedPiece is not null)
            {
                highlightGrabbedMoves = true;
            }
            UpdateGameDisplay();
        }

        private async void evaluation_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (currentBestMove is not null || game.GameOver
                || (game.CurrentTurnWhite && whiteIsComputer) || (!game.CurrentTurnWhite && blackIsComputer))
            {
                return;
            }
            manuallyEvaluating = true;
            grabbedPiece = null;
            highlightGrabbedMoves = false;
            UpdateEvaluationMeter(null, game.CurrentTurnWhite);
            UpdateGameDisplay();
            UpdateCursor(e.GetPosition(chessGameCanvas));

            CancellationToken cancellationToken = cancelMoveComputation.Token;
            BoardAnalysis.PossibleMove bestMove = await GetEngineMove(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            UpdateEvaluationMeter(bestMove, game.CurrentTurnWhite);
            currentBestMove = bestMove;
            UpdateGameDisplay();
            manuallyEvaluating = false;
        }

        private async void NewGame_Click(object sender, RoutedEventArgs e)
        {
            whiteIsComputer = false;
            blackIsComputer = false;
            await NewGame();
        }

        private async void NewGameCpuWhite_Click(object sender, RoutedEventArgs e)
        {
            whiteIsComputer = false;
            blackIsComputer = true;
            await NewGame();
        }

        private async void NewGameCpuBlack_Click(object sender, RoutedEventArgs e)
        {
            whiteIsComputer = true;
            blackIsComputer = false;
            await NewGame();
        }

        private async void NewGameCpuOnly_Click(object sender, RoutedEventArgs e)
        {
            whiteIsComputer = true;
            blackIsComputer = true;
            await NewGame();
        }

        private void Window_Closing(object sender, WindowClosingEventArgs e)
        {
            cancelMoveComputation.Cancel();
            string jsonPath = System.IO.Path.Join(AppDomain.CurrentDomain.BaseDirectory, "chess-settings.json");
            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(config));
        }

        private async void PGNExport_Click(object sender, RoutedEventArgs e)
        {
            manuallyEvaluating = false;
            cancelMoveComputation.Cancel();
            cancelMoveComputation = new CancellationTokenSource();
            await new PGNExport(game, whiteIsComputer, blackIsComputer).ShowDialog(this);
            await CheckComputerMove();
        }

        private async void CustomGame_Click(object sender, RoutedEventArgs e)
        {
            manuallyEvaluating = false;
            cancelMoveComputation.Cancel();
            cancelMoveComputation = new CancellationTokenSource();
            CustomGame customDialog = new();
            await customDialog.ShowDialog(this);
            if (customDialog.GeneratedGame is not null)
            {
                game = customDialog.GeneratedGame;
                whiteIsComputer = customDialog.WhiteIsComputer;
                blackIsComputer = customDialog.BlackIsComputer;
                grabbedPiece = null;
                highlightGrabbedMoves = false;
                currentBestMove = null;
                whiteEvaluation.Content = "?";
                blackEvaluation.Content = "?";
                UpdateGameDisplay();
                await PushEndgameMessage();
            }
            await CheckComputerMove();
        }

        private void SettingsCheckItem_Click(object sender, RoutedEventArgs e)
        {
            config.AutoQueen = autoQueenItem.IsChecked;
            config.UseSymbolsOnMoveList = moveListSymbolsItem.IsChecked;
            config.FlipBoard = flipBoardItem.IsChecked;
            config.UpdateEvalAfterBot = updateEvalAfterBotItem.IsChecked;
            config.ExternalEngineWhite = externalEngineWhiteItem.IsChecked;
            config.ExternalEngineBlack = externalEngineBlackItem.IsChecked;

            if (config.ExternalEngineWhite || config.ExternalEngineBlack)
            {
                foreach (string filename in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.exe"))
                {
                    if (System.IO.Path.GetFileNameWithoutExtension(filename) != Process.GetCurrentProcess().ProcessName)
                    {
                        enginePath = filename;
                        break;
                    }
                }
            }
            else
            {
                enginePath = null;
            }
            UpdateGameDisplay();
        }

        private async void FENCopy_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard is null)
            {
                return;
            }
            await Clipboard.SetTextAsync(game.ToString());
        }

        private async void CustomiseItem_Click(object sender, RoutedEventArgs e)
        {
            await new Customisation(config).ShowDialog(this);
            rectSizeReference.Fill = new SolidColorBrush(config.DarkSquareColor);
            chessBoardBackground.Background = new SolidColorBrush(config.LightSquareColor);
            UpdateGameDisplay();
        }

        private async void UndoMove_Click(object sender, RoutedEventArgs e)
        {
            if (game.PreviousGameState is not null
                && ((game.CurrentTurnWhite && !whiteIsComputer) || (!game.CurrentTurnWhite && !blackIsComputer)))
            {
                game = game.PreviousGameState;
                if (whiteIsComputer ||  blackIsComputer)
                {
                    // Reverse two moves if the opponent is computer controlled
                    game = game.PreviousGameState!;
                }

                grabbedPiece = null;
                highlightGrabbedMoves = false;
                currentBestMove = null;
                whiteEvaluation.Content = "?";
                blackEvaluation.Content = "?";
                manuallyEvaluating = false;
                cancelMoveComputation.Cancel();
                cancelMoveComputation = new CancellationTokenSource();

                UpdateGameDisplay();
                await CheckComputerMove();
            }
        }

        private void WhiteDepthBackingSlider_Click(object? sender, RoutedEventArgs e)
        {
            if (!updateDepthSliders || sender is not Control { Tag: string tag })
            {
                return;
            }
            config.ExternalEngineWhiteDepth = uint.Parse(tag);
        }

        private void BlackDepthBackingSlider_Click(object? sender, RoutedEventArgs e)
        {
            if (!updateDepthSliders || sender is not Control { Tag: string tag })
            {
                return;
            }
            config.ExternalEngineBlackDepth = uint.Parse(tag);
        }
    }
}
