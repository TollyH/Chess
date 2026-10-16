using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Chess
{
    /// <summary>
    /// Interaction logic for PGNExport.xaml
    /// </summary>
    public partial class PGNExport : Window
    {
        private readonly ChessGame game;
        private readonly bool whiteIsComputer;
        private readonly bool blackIsComputer;

        public PGNExport(ChessGame game, bool whiteIsComputer, bool blackIsComputer)
        {
            this.game = game;
            this.whiteIsComputer = whiteIsComputer;
            this.blackIsComputer = blackIsComputer;
            InitializeComponent();

            if (whiteIsComputer)
            {
                whiteNameBox.Text = "Computer";
                whiteNameBox.IsReadOnly = true;
                whiteNameBox.IsEnabled = false;
            }
            if (blackIsComputer)
            {
                blackNameBox.Text = "Computer";
                blackNameBox.IsReadOnly = true;
                blackNameBox.IsEnabled = false;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            IStorageFile? file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                DefaultExtension = ".pgn",
                FileTypeChoices = new List<FilePickerFileType>()
                {
                    new("Portable Game Notation") { Patterns = new List<string>() { "*.pgn"} }
                }
            });
            if (file is null)
            {
                return;
            }
            string eventName = eventNameBox.Text?.Trim() ?? "";
            string locationName = locationNameBox.Text?.Trim() ?? "";
            DateOnly? date = dateBox.SelectedDate is null ? null : DateOnly.FromDateTime(dateBox.SelectedDate.Value.DateTime);
            string whiteName = whiteNameBox.Text?.Trim() ?? "";
            string blackName = blackNameBox.Text?.Trim() ?? "";
            File.WriteAllText(file.Path.LocalPath, game.ToPGN(eventName != "" ? eventName : null,
                locationName != "" ? locationName : null, date, whiteName != "" ? whiteName : "Player",
                blackName != "" ? blackName : "Player", whiteIsComputer, blackIsComputer));
            Close();
        }
    }
}
