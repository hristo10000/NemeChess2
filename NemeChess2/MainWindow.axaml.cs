using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using NemeChess2.ViewModels;
using System;
using Avalonia.Data.Converters;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Media;
using System.Globalization;

namespace NemeChess2
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string imagePath)
            {
                return new Avalonia.Media.Imaging.Bitmap(imagePath);
            }
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }

    public class HighlightColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isHighlighted && isHighlighted)
            {
                return Brushes.Yellow;
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Brushes.Transparent;
        }
    }
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow(IServiceProvider provider)
        {
            InitializeComponent();
            _viewModel = provider.GetRequiredService<MainViewModel>();
            _viewModel.GameOver += GameOverWindow;
        }

        private void GameOverWindow(object? sender, EventArgs e)
        {
            var popup = new CustomPopUp(_viewModel.IsMyTurn ? "You Loose!" : "You Win!");
            popup.ShowDialog(this);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void Square_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var border = (Border)sender;
            var square = (ChessSquare)border.DataContext;

            if (_viewModel.SelectedSquare == null)
            {
                _viewModel.SelectedSquare = square;
                square.IsSelected = true;
            }
            else
            {
                var move = _viewModel.SelectedSquare.SquareName + square.SquareName;
                await _viewModel.MakeMoveAsync(move);
                _viewModel.SelectedSquare.IsSelected = false;
                _viewModel.SelectedSquare = null;
            }
        }
    }
}
