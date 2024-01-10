using Avalonia.Media;
using System;
using System.ComponentModel;
using System.IO;

namespace NemeChess2.ViewModels
{
    public class ChessSquare : INotifyPropertyChanged
    {
        private IBrush? _background;
        private string? _piece;
        private bool _isSelected;
        private string? _squareName;
        public string? SquareName
        {
            get { return _squareName; }
            set
            {
                if (_squareName != value)
                {
                    _squareName = value;
                    OnPropertyChanged(nameof(SquareName));
                }
            }
        }

        private bool _isHighlighted;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                if (_isHighlighted != value)
                {
                    _isHighlighted = value;
                    OnPropertyChanged(nameof(IsHighlighted));
                }
            }
        }
        private Color _highlightColor;
        public Color HighlightColor
        {
            get { return _highlightColor; }
            set
            {
                if (_highlightColor != value)
                {
                    _highlightColor = value;
                    OnPropertyChanged(nameof(HighlightColor));
                }
            }
        }
        public IBrush? Background
        {
            get { return _background; }
             set
            {
                if (_background != value)
                {
                    _background = value;
                    OnPropertyChanged(nameof(Background));
                }
            }
        }

        private string? _pieceImageSource;
        public string? PieceImageSource
        {
            get { return _pieceImageSource; }
            set
            {
                if (_pieceImageSource != value)
                {
                    _pieceImageSource = value;
                    OnPropertyChanged(nameof(PieceImageSource));
                }
            }
        }
        public string? Piece
        {
            get { return _piece; }
            set
            {
                if (_piece != value)
                {
                    _piece = value;
                    UpdatePieceImageSource();
                    OnPropertyChanged(nameof(Piece));
                }
            }
        }
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void UpdatePieceImageSource()
        {
            if (!string.IsNullOrEmpty(Piece))
            {
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pieces-basic", $"{Piece}.png");
                PieceImageSource = imagePath;
            }
            else
            {
                PieceImageSource = null;
            }

        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
