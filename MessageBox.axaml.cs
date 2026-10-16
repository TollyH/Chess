using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Chess
{
    public enum MessageBoxImage
    {
        Error,
        Warning,
        Information,
        Question
    }

    public enum MessageBoxButton
    {
        OK,
        YesNo,
        YesNoCancel
    }

    public enum MessageBoxResult
    {
        None,
        OK,
        Yes,
        No,
        Cancel
    }

    public partial class MessageBox : Window
    {
        private static readonly Bitmap errorImage = new(AssetLoader.Open(new Uri("avares://Chess/Resources/error.png")));
        private static readonly Bitmap warningImage = new(AssetLoader.Open(new Uri("avares://Chess/Resources/warning.png")));
        private static readonly Bitmap informationImage = new(AssetLoader.Open(new Uri("avares://Chess/Resources/information.png")));
        private static readonly Bitmap questionImage = new(AssetLoader.Open(new Uri("avares://Chess/Resources/question.png")));

        private readonly MessageBoxButton buttonLayout;

        public MessageBox(string message, string title, MessageBoxButton button, MessageBoxImage image)
        {
            InitializeComponent();

            messageText.Text = message;
            Title = title;
            imageIcon.Source = image switch
            {
                MessageBoxImage.Error => errorImage,
                MessageBoxImage.Warning => warningImage,
                MessageBoxImage.Information => informationImage,
                MessageBoxImage.Question => questionImage,
                _ => throw new ArgumentException("Invalid message box image", nameof(image))
            };

            buttonLayout = button;
            switch (button)
            {
                case MessageBoxButton.YesNo:
                    button1.Content = "No";
                    button2.IsVisible = true;
                    break;
                case MessageBoxButton.YesNoCancel:
                    button1.Content = "Cancel";
                    button2.Content = "No";
                    button2.IsVisible = true;
                    button3.IsVisible = true;
                    break;
            }
        }

        private void Button1_Click(object? sender, RoutedEventArgs e)
        {
            Close(buttonLayout switch
            {
                MessageBoxButton.OK => MessageBoxResult.OK,
                MessageBoxButton.YesNo => MessageBoxResult.No,
                MessageBoxButton.YesNoCancel => MessageBoxResult.Cancel,
                _ => MessageBoxResult.None
            });
        }

        private void Button2_Click(object? sender, RoutedEventArgs e)
        {
            Close(buttonLayout switch
            {
                MessageBoxButton.YesNo => MessageBoxResult.Yes,
                MessageBoxButton.YesNoCancel => MessageBoxResult.No,
                _ => MessageBoxResult.None
            });
        }

        private void Button3_Click(object? sender, RoutedEventArgs e)
        {
            Close(buttonLayout switch
            {
                MessageBoxButton.YesNoCancel => MessageBoxResult.Yes,
                _ => MessageBoxResult.None
            });
        }
    }
}
