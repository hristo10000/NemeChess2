﻿using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using NemeChess2.ViewModels;
using System.Diagnostics;
using System;
using Avalonia.Data.Converters;

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

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
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
                if (_viewModel.SelectedSquare != null)
                {
                    var move = $"{Convert.ToChar('a' + _viewModel.SelectedSquare.Column)}{_viewModel.SelectedSquare.Row + 1}{Convert.ToChar('a' + square.Column)}{square.Row + 1}";
                    Debug.WriteLine(move);
                    await _viewModel.MakeMoveAsync(move);
                    _viewModel.SelectedSquare.IsSelected = false;
                    _viewModel.SelectedSquare = null;
                }
            }
        }
    }
}
