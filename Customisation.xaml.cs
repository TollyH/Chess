using System.Windows;
using System.Windows.Media;

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

            lightSquarePicker.SelectedColor = Config.LightSquareColor;
            darkSquarePicker.SelectedColor = Config.DarkSquareColor;
            defaultPiecePicker.SelectedColor = Config.DefaultPieceColor;
            checkKingPicker.SelectedColor = Config.CheckedKingColor;
            selectedPiecePicker.SelectedColor = Config.SelectedPieceColor;
            checkmatePicker.SelectedColor = Config.CheckMateHighlightColor;
            lastMoveSourcePicker.SelectedColor = Config.LastMoveSourceColor;
            lastMoveDestinationPicker.SelectedColor = Config.LastMoveDestinationColor;
            bestMoveSourcePicker.SelectedColor = Config.BestMoveSourceColor;
            bestMoveDestinationPicker.SelectedColor = Config.BestMoveDestinationColor;
            availableMovePicker.SelectedColor = Config.AvailableMoveColor;
            availableCapturePicker.SelectedColor = Config.AvailableCaptureColor;
            availableEnPassantPicker.SelectedColor = Config.AvailableEnPassantColor;
            availableCastlePicker.SelectedColor = Config.AvailableCastleColor;

            performRefresh = true;
        }

        private void Picker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (!performRefresh)
            {
                return;
            }
            Config.LightSquareColor = lightSquarePicker.SelectedColor ?? default;
            Config.DarkSquareColor = darkSquarePicker.SelectedColor ?? default;
            Config.DefaultPieceColor = defaultPiecePicker.SelectedColor ?? default;
            Config.CheckedKingColor = checkKingPicker.SelectedColor ?? default;
            Config.SelectedPieceColor = selectedPiecePicker.SelectedColor ?? default;
            Config.CheckMateHighlightColor = checkmatePicker.SelectedColor ?? default;
            Config.LastMoveSourceColor = lastMoveSourcePicker.SelectedColor ?? default;
            Config.LastMoveDestinationColor = lastMoveDestinationPicker.SelectedColor ?? default;
            Config.BestMoveSourceColor = bestMoveSourcePicker.SelectedColor ?? default;
            Config.BestMoveDestinationColor = bestMoveDestinationPicker.SelectedColor ?? default;
            Config.AvailableMoveColor = availableMovePicker.SelectedColor ?? default;
            Config.AvailableCaptureColor = availableCapturePicker.SelectedColor ?? default;
            Config.AvailableEnPassantColor = availableEnPassantPicker.SelectedColor ?? default;
            Config.AvailableCastleColor = availableCastlePicker.SelectedColor ?? default;
        }
    }
}
