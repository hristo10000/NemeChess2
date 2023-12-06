using Avalonia;
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
using Avalonia.Threading;
using System.Linq;

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
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void UpdatePieceImageSource()
        {
            if (!string.IsNullOrEmpty(Piece))
            {
                string absolutePath = @"C:\Users\Hristo Ivanov\Documents\GitHub\NemeChess2\NemeChess2\bin\Debug\net6.0-windows\pieces-basic\";

                PieceImageSource = Path.Combine(absolutePath, $"{Piece}.png");
            }
        }
    }
    public partial class MainWindow : Window
    {
        private readonly LichessApiService _lichessApiService;
        private ChessSquare? _selectedSquare;
        private static string? GameId;
        private List<ChessSquare> _chessboard = new List<ChessSquare>();
        private GameStreamingService _gameStreamingService;
        public bool isWhite { get; set; }
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
        private bool DetermineIsWhite(LichessGame lichessGame)
        {
            return lichessGame?.Player?.ToLower() == "white";
        }
        public MainWindow()
        {
            InitializeComponent();

            _lichessApiService = new LichessApiService("lip_JYrY3vNv29oHJWFb4XLN");

            Chessboard = GenerateChessboard();

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
                        await _gameStreamingService.StartStreamingAsync();

                        isWhite = DetermineIsWhite(lichessGame);
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
                    Dispatcher.UIThread.InvokeAsync(() => UpdateChessboard(gameUpdate.State.Moves));
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


        private void UpdateChessboard(string moves)
        {
            Chessboard.Clear();
            Chessboard.AddRange(GenerateChessboard());

            var moveList = moves.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var move in moveList)
            {
                var fromSquare = move.Substring(0, 2);
                var toSquare = move.Substring(2, 2);
                MovePiece(int.Parse(fromSquare[1].ToString()) - 1, fromSquare[0] - 'a',
                          int.Parse(toSquare[1].ToString()) - 1, toSquare[0] - 'a');
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
            }
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        private void Square_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var border = (Border)sender;
            var square = (ChessSquare)border.DataContext;
            if (_selectedSquare == null)
            {
                _selectedSquare = square;
                square.IsSelected = true;
                border.BorderBrush = Brushes.Red;
                border.BorderThickness = new Thickness(2);
            }
            else
            {
                var move = $"{Convert.ToChar('a' + _selectedSquare.Column)}{_selectedSquare.Row + 1}{Convert.ToChar('a' + square.Column)}{square.Row + 1}";
                Debug.WriteLine(move);
                MakeMoveAsync(move);
                _selectedSquare.IsSelected = false;
                _selectedSquare = null;
                border.BorderBrush = Brushes.Black;
                border.BorderThickness = new Thickness(1);
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
        /*private static List<ChessSquare> GenerateChessboard(bool isWhitePlayer)
        {
            var chessboard = new List<ChessSquare>();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var square = new ChessSquare
                    {
                        Background = (row + col) % 2 == 0 ? Brushes.White : Brushes.LightGray,
                        Piece = GetInitialPiece(row, col, isWhitePlayer),
                        Row = isWhitePlayer ? row : 7 - row,
                        Column = isWhitePlayer ? col : 7 - col
                    };
                    chessboard.Add(square);
                }
            }

            return chessboard;
        }*/
        private List<ChessSquare> GenerateChessboard()// separate method for putting pieces
        {
            var chessboard = new List<ChessSquare>();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var square = new ChessSquare
                    {
                        Background = (row + col) % 2 == 0 ? Brushes.White : Brushes.LightGray,
                        Piece = GetInitialPiece(row, col),
                        Row = row,
                        Column = col
                    };
                    chessboard.Add(square);
                }
            }
            return chessboard;
        }
        /* private static string GetInitialPiece(int row, int col, bool isWhite)
         {
             string color = isWhite ? "white" : "black";

             if (row == 1 || row == 6)
             {
                 return $"{color}-pawn";
             }
             if (row == 0 || row == 7)
             {
                 switch (col)
                 {
                     case 0:
                     case 7:
                         return $"{color}-rook";
                     case 1:
                     case 6:
                         return $"{color}-knight";
                     case 2:
                     case 5:
                         return $"{color}-bishop";
                     case 3:
                         return $"{color}-queen";
                     case 4:
                         return $"{color}-king";
                 }
             }
             return "";
         }
 */
        private static string GetInitialPiece(int row, int col)
        {
            string color = (row == 0 || row == 1) ? "black" : "white";

            if (row == 1 || row == 6)
            {
                return $"{color}-pawn";
            }
            if (row == 0 || row == 7)
            {
                switch (col)
                {
                    case 0:
                    case 7:
                        return $"{color}-rook";
                    case 1:
                    case 6:
                        return $"{color}-knight";
                    case 2:
                    case 5:
                        return $"{color}-bishop";
                    case 3:
                        return $"{color}-queen";
                    case 4:
                        return $"{color}-king";
                }
            }
            return "";
        }

    }
}