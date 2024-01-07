using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using NemeChess2.ViewModels;
using System;
using Avalonia.Data.Converters;
using Microsoft.Extensions.DependencyInjection;

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
            throw new NotImplementedException();
        }
    }
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        public MainWindow(IServiceProvider provider)
        {
            InitializeComponent();
            _viewModel = provider.GetRequiredService<MainViewModel>();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        private async void Square_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            GameOverWindow();
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
        public void GameOverWindow()
        {
            //var popup = new CustomPopUp("You Loose!");
            //popup.ShowDialog(this);
        }
    }
}
