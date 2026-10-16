using Avalonia.Controls;

namespace Chess
{
    /// <summary>
    /// Interaction logic for Customisation.xaml
    /// </summary>
    public partial class Customisation : Window
    {
        public Settings Config { get; }

        private bool performRefresh = false;

        public Customisation(Settings config)
        {
            Config = config;
            InitializeComponent();

            lightSquarePicker.Color = Config.LightSquareColor;
            darkSquarePicker.Color = Config.DarkSquareColor;
            defaultPiecePicker.Color = Config.DefaultPieceColor;
            checkKingPicker.Color = Config.CheckedKingColor;
            selectedPiecePicker.Color = Config.SelectedPieceColor;
            checkmatePicker.Color = Config.CheckMateHighlightColor;
            lastMoveSourcePicker.Color = Config.LastMoveSourceColor;
            lastMoveDestinationPicker.Color = Config.LastMoveDestinationColor;
            bestMoveSourcePicker.Color = Config.BestMoveSourceColor;
            bestMoveDestinationPicker.Color = Config.BestMoveDestinationColor;
            availableMovePicker.Color = Config.AvailableMoveColor;
            availableCapturePicker.Color = Config.AvailableCaptureColor;
            availableEnPassantPicker.Color = Config.AvailableEnPassantColor;
            availableCastlePicker.Color = Config.AvailableCastleColor;

            performRefresh = true;
        }

        private void Picker_ColorChanged(object sender, ColorChangedEventArgs e)
        {
            if (!performRefresh)
            {
                return;
            }
            Config.LightSquareColor = lightSquarePicker.Color;
            Config.DarkSquareColor = darkSquarePicker.Color;
            Config.DefaultPieceColor = defaultPiecePicker.Color;
            Config.CheckedKingColor = checkKingPicker.Color;
            Config.SelectedPieceColor = selectedPiecePicker.Color;
            Config.CheckMateHighlightColor = checkmatePicker.Color;
            Config.LastMoveSourceColor = lastMoveSourcePicker.Color;
            Config.LastMoveDestinationColor = lastMoveDestinationPicker.Color;
            Config.BestMoveSourceColor = bestMoveSourcePicker.Color;
            Config.BestMoveDestinationColor = bestMoveDestinationPicker.Color;
            Config.AvailableMoveColor = availableMovePicker.Color;
            Config.AvailableCaptureColor = availableCapturePicker.Color;
            Config.AvailableEnPassantColor = availableEnPassantPicker.Color;
            Config.AvailableCastleColor = availableCastlePicker.Color;
        }
    }
}
