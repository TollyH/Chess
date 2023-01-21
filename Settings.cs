using Newtonsoft.Json;
using System;
using System.Windows.Media;

namespace Chess
{
    [Serializable]
    public class Settings
    {
        public bool AutoQueen { get; set; }
        public bool UseSymbolsOnMoveList { get; set; }
        public bool FlipBoard { get; set; }
        public bool UpdateEvalAfterBot { get; set; }
        public bool ExternalEngineWhite { get; set; }
        public bool ExternalEngineBlack { get; set; }
        public uint ExternalEngineWhiteDepth { get; set; }
        public uint ExternalEngineBlackDepth { get; set; }

        public Color LightSquareColor { get; set; }
        public Color DarkSquareColor { get; set; }
        public Color DefaultPieceColor { get; set; }
        public Color CheckedKingColor { get; set; }
        public Color SelectedPieceColor { get; set; }
        public Color CheckMateHighlightColor { get; set; }
        public Color LastMoveSourceColor { get; set; }
        public Color LastMoveDestinationColor { get; set; }
        public Color BestMoveSourceColor { get; set; }
        public Color BestMoveDestinationColor { get; set; }
        public Color AvailableMoveColor { get; set; }
        public Color AvailableCaptureColor { get; set; }
        public Color AvailableEnPassantColor { get; set; }
        public Color AvailableCastleColor { get; set; }

        public Settings()
        {
            AutoQueen = false;
            UseSymbolsOnMoveList = false;
            FlipBoard = false;
            UpdateEvalAfterBot = true;
            ExternalEngineWhite = false;
            ExternalEngineBlack = false;
            ExternalEngineWhiteDepth = 24;
            ExternalEngineBlackDepth = 24;

            LightSquareColor = Brushes.White.Color;
            DarkSquareColor = Color.FromRgb(191, 130, 69);
            DefaultPieceColor = Brushes.Black.Color;
            CheckedKingColor = Brushes.Red.Color;
            SelectedPieceColor = Brushes.Blue.Color;
            CheckMateHighlightColor = Brushes.IndianRed.Color;
            LastMoveSourceColor = Brushes.CadetBlue.Color;
            LastMoveDestinationColor = Brushes.Cyan.Color;
            BestMoveSourceColor = Brushes.LightGreen.Color;
            BestMoveDestinationColor = Brushes.Green.Color;
            AvailableMoveColor = Brushes.Yellow.Color;
            AvailableCaptureColor = Brushes.Red.Color;
            AvailableEnPassantColor = Brushes.OrangeRed.Color;
            AvailableCastleColor = Brushes.MediumPurple.Color;
        }

        [JsonConstructor]
        public Settings(bool autoQueen, bool useSymbolsOnMoveList, bool flipBoard, bool externalEngineWhite, bool externalEngineBlack, bool updateEvalAfterBot,
            uint externalEngineWhiteDepth, uint externalEngineBlackDepth, Color lightSquareColor, Color darkSquareColor, Color defaultPieceColor,
            Color checkedKingColor, Color selectedPieceColor, Color checkMateHighlightColor, Color lastMoveSourceColor, Color lastMoveDestinationColor,
            Color bestMoveSourceColor, Color bestMoveDestinationColor, Color availableMoveColor, Color availableCaptureColor, Color availableEnPassantColor,
            Color availableCastleColor)
        {
            AutoQueen = autoQueen;
            UseSymbolsOnMoveList = useSymbolsOnMoveList;
            FlipBoard = flipBoard;
            UpdateEvalAfterBot = updateEvalAfterBot;
            ExternalEngineWhite = externalEngineWhite;
            ExternalEngineBlack = externalEngineBlack;
            ExternalEngineWhiteDepth = externalEngineWhiteDepth;
            ExternalEngineBlackDepth = externalEngineBlackDepth;
            LightSquareColor = lightSquareColor;
            DarkSquareColor = darkSquareColor;
            DefaultPieceColor = defaultPieceColor;
            CheckedKingColor = checkedKingColor;
            SelectedPieceColor = selectedPieceColor;
            CheckMateHighlightColor = checkMateHighlightColor;
            LastMoveSourceColor = lastMoveSourceColor;
            LastMoveDestinationColor = lastMoveDestinationColor;
            BestMoveSourceColor = bestMoveSourceColor;
            BestMoveDestinationColor = bestMoveDestinationColor;
            AvailableMoveColor = availableMoveColor;
            AvailableCaptureColor = availableCaptureColor;
            AvailableEnPassantColor = availableEnPassantColor;
            AvailableCastleColor = availableCastleColor;
        }
    }
}
