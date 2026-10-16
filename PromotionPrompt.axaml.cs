using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Chess
{
    /// <summary>
    /// Interaction logic for PromotionPrompt.xaml
    /// </summary>
    public partial class PromotionPrompt : Window
    {
        /// <remarks>
        /// Defaults to <see cref="Pieces.Queen"/>
        /// </remarks>
        public Type ChosenPieceType { get; private set; }

        public PromotionPrompt(bool isWhite)
        {
            ChosenPieceType = typeof(Pieces.Queen);

            InitializeComponent();

            queenButtonLabel.Content = isWhite ? "♕" : "♛";
            rookButtonLabel.Content = isWhite ? "♖" : "♜";
            bishopButtonLabel.Content = isWhite ? "♗" : "♝";
            knightButtonLabel.Content = isWhite ? "♘" : "♞";
        }

        private void queenButton_Click(object sender, RoutedEventArgs e)
        {
            ChosenPieceType = typeof(Pieces.Queen);
            Close(ChosenPieceType);
        }

        private void rookButton_Click(object sender, RoutedEventArgs e)
        {
            ChosenPieceType = typeof(Pieces.Rook);
            Close(ChosenPieceType);
        }

        private void bishopButton_Click(object sender, RoutedEventArgs e)
        {
            ChosenPieceType = typeof(Pieces.Bishop);
            Close(ChosenPieceType);
        }

        private void knightButton_Click(object sender, RoutedEventArgs e)
        {
            ChosenPieceType = typeof(Pieces.Knight);
            Close(ChosenPieceType);
        }
    }
}
