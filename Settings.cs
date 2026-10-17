using Avalonia.Media;

namespace Chess
{
    public class Settings
    {
        public bool AutoQueen { get; set; } = false;
        public bool UseSymbolsOnMoveList { get; set; } = false;
        public bool FlipBoard { get; set; } = false;
        public bool UpdateEvalAfterBot { get; set; } = true;
        public bool ExternalEngineWhite { get; set; } = false;
        public bool ExternalEngineBlack { get; set; } = false;
        public uint ExternalEngineWhiteDepth { get; set; } = 24;
        public uint ExternalEngineBlackDepth { get; set; } = 24;

        public Color LightSquareColor { get; set; } = Brushes.White.Color;
        public Color DarkSquareColor { get; set; } = Color.FromRgb(191, 130, 69);
        public Color DefaultPieceColor { get; set; } = Brushes.Black.Color;
        public Color CheckedKingColor { get; set; } = Brushes.Red.Color;
        public Color SelectedPieceColor { get; set; } = Brushes.Blue.Color;
        public Color CheckMateHighlightColor { get; set; } = Brushes.IndianRed.Color;
        public Color LastMoveSourceColor { get; set; } = Brushes.CadetBlue.Color;
        public Color LastMoveDestinationColor { get; set; } = Brushes.Cyan.Color;
        public Color BestMoveSourceColor { get; set; } = Brushes.LightGreen.Color;
        public Color BestMoveDestinationColor { get; set; } = Brushes.Green.Color;
        public Color AvailableMoveColor { get; set; } = Brushes.Yellow.Color;
        public Color AvailableCaptureColor { get; set; } = Brushes.Red.Color;
        public Color AvailableEnPassantColor { get; set; } = Brushes.OrangeRed.Color;
        public Color AvailableCastleColor { get; set; } = Brushes.MediumPurple.Color;
    }
}
