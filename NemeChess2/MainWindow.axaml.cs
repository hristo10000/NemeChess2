using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Avalonia.Data.Converters;
using NemeChess2.Models;
using System.Linq;
using DynamicData;

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
    public class ChessSquare : INotifyPropertyChanged
    {
        private IBrush? _background;
        private string? _piece;
        private bool _isSelected;
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
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void UpdatePieceImageSource()
        {
            if (!string.IsNullOrEmpty(Piece))
            { 
                PieceImageSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pieces-basic", $"{Piece}.png");
                Debug.WriteLine(PieceImageSource);
            }
            OnPropertyChanged(nameof(PieceImageSource));
        }
    }
    public partial class MainWindow : Window
    {
        private readonly LichessApiService _lichessApiService;
        private ChessSquare? _selectedSquare;
        private static string? GameId;
        private List<ChessSquare> _chessboard = new List<ChessSquare>();
        private GameStreamingService _gameStreamingService;
        public bool IsWhite { get; set; }
        public LichessGame CurrentGame;
        public List<ChessSquare> Chessboard
        {
            get { return _chessboard; }
            set
            {
                if (_chessboard != value)
                {
                    _chessboard = value;
                    OnPropertyChanged(nameof(Chessboard));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public MainWindow()
        {
            InitializeComponent();
            _lichessApiService = new LichessApiService("lip_JYrY3vNv29oHJWFb4XLN");
            Chessboard = new List<ChessSquare>();
            Task.Run(async () =>
            {
                try
                {
                    var lichessGame = await _lichessApiService.CreateBotGameAsync();

                    if (lichessGame != null && !string.IsNullOrEmpty(lichessGame.Id))
                    {
                        GameId = lichessGame.Id;
                        Debug.WriteLine($"Bot game created! Game ID: {GameId}");

                        _gameStreamingService = new GameStreamingService("lip_JYrY3vNv29oHJWFb4XLN", GameId, HandleGameUpdate);
                        IsWhite = await _gameStreamingService.GetInitialResponse();
                        Debug.WriteLine(IsWhite ? "You're White!" : "You're Black");
                        Chessboard = GenerateChessboard(IsWhite);
                    }
                    else
                    {
                        Debug.WriteLine("Failed to create a bot game. Check the console for details.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"An error occurred: {ex.Message}");
                }
            });
            DataContext = this;
        }
        private void HandleGameUpdate(GameUpdate gameUpdate)
        {
            if (gameUpdate != null && gameUpdate.State != null)
            {
                if (gameUpdate.State.Moves != null)
                {
                    UpdateChessboard(gameUpdate.State.Moves);
                }
                else
                {
                    Console.WriteLine("Moves property is null in game update.");
                }
            }
            else
            {
                Console.WriteLine("GameUpdate or State is null in game update.");
            }
        }
        public void UpdateChessboard(string moves)
        {
            if (Chessboard == null)
            {
                Debug.WriteLine("Chessboard is null");
                return;
            }
            var moveList = moves.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var move in moveList)
            {
                if (move.Length == 4)
                {
                    var fromSquare = Chessboard.FirstOrDefault(square => square.Row == (move[1] - '1') && square.Column == (move[0] - 'a'));
                    var toSquare = Chessboard.FirstOrDefault(square => square.Row == (move[3] - '1') && square.Column == (move[2] - 'a'));
                    if (fromSquare != null && toSquare != null)
                    {
                        MovePiece(fromSquare.Row, fromSquare.Column, toSquare.Row, toSquare.Column);
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid move format: {move}");
                }
            }
        }
        private void MovePiece(int fromRow, int fromCol, int toRow, int toCol)
        {
            var fromSquare = Chessboard.FirstOrDefault(square => square.Row == fromRow && square.Column == fromCol);
            var toSquare = Chessboard.FirstOrDefault(square => square.Row == toRow && square.Column == toCol);
            if (fromSquare != null && toSquare != null)
            {
                toSquare.Piece = fromSquare.Piece;
                fromSquare.Piece = "";

                //bad
                toSquare.UpdatePieceImageSource();
                fromSquare.UpdatePieceImageSource();

                //???
                OnPropertyChanged(nameof(fromSquare));
                OnPropertyChanged(nameof(toSquare));
            }
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        private async void Square_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var border = (Border)sender;
            var square = (ChessSquare)border.DataContext;
            if (_selectedSquare == null)
            {
                _selectedSquare = square;
                square.IsSelected = true;
            }
            else
            {
                if (_selectedSquare != null)
                {
                    var move = $"{Convert.ToChar('a' + _selectedSquare.Column)}{_selectedSquare.Row + 1}{Convert.ToChar('a' + square.Column)}{square.Row + 1}";
                    Debug.WriteLine(move);
                    await MakeMoveAsync(move);
                    _selectedSquare.IsSelected = false;
                    _selectedSquare = null;
                }
            }
        }
        private async Task MakeMoveAsync(string move)
        {
            try
            {
                await _lichessApiService.MakeMoveAsync(GameId, move); 
                Console.WriteLine($"Move {move} played successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error making move: {ex.Message}");
            }
        }
        public List<ChessSquare> GenerateChessboard(bool isWhitePlayer)
        {
            for (int row = 0; row < 8; row++)
            {
                List<ChessSquare> rowSquares = new List<ChessSquare>();

                for (int col = 0; col < 8; col++)
                {
                    ChessSquare square = new ()
                    {
                        Background = (row + col) % 2 == 0 ? Brushes.White : Brushes.LightGray,
                        Piece = GetInitialPiece(row, col, isWhitePlayer),
                        Row = row,
                        Column = col
                    };
                    rowSquares.Add(square);
                }
                Chessboard.Add(rowSquares);
            }
            return Chessboard;
        }
        public static string GetInitialPiece(int row, int col, bool isWhite)
        {
            if (row < 0 || row > 7 || col < 0 || col > 7)
            {
                return "Invalid position";
            }

            if (isWhite)
            {
                switch (row)
                {
                    case 0:
                        switch (col)
                        {
                            case 0: return "black-rook";
                            case 1: return "black-knight";
                            case 2: return "black-bishop";
                            case 3: return "black-queen";
                            case 4: return "black-king";
                            case 5: return "black-bishop";
                            case 6: return "black-knight";
                            case 7: return "black-rook";
                        }
                        break;
                    case 1: return "black-pawn";
                    case 6: return "white-pawn";
                    case 7:
                        switch (col)
                        {
                            case 0: return "white-rook";
                            case 1: return "white-knight";
                            case 2: return "white-bishop";
                            case 3: return "white-queen";
                            case 4: return "white-king";
                            case 5: return "white-bishop";
                            case 6: return "white-knight";
                            case 7: return "white-rook";
                        }
                        break;
                }
            }
            else
            {
                switch (row)
                {
                    case 0:
                        switch (col)
                        {
                            case 0: return "white-rook";
                            case 1: return "white-knight";
                            case 2: return "white-bishop";
                            case 3: return "white-queen";
                            case 4: return "white-king";
                            case 5: return "white-bishop";
                            case 6: return "white-knight";
                            case 7: return "white-rook";
                        }
                        break;
                    case 1: return "white-pawn";
                    case 6: return "black-pawn";
                    case 7:
                        switch (col)
                        {
                            case 0: return "black-rook";
                            case 1: return "black-knight";
                            case 2: return "black-bishop";
                            case 3: return "black-queen";
                            case 4: return "black-king";
                            case 5: return "black-bishop";
                            case 6: return "black-knight";
                            case 7: return "black-rook";
                        }
                        break;
                }
            }

            return "";
        }
    }
}
