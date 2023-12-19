using NemeChess2.Models;
using NemeChess2.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using DynamicData;
using Avalonia.Media;
using Avalonia.Threading;

namespace NemeChess2
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly LichessApiService _lichessApiService;
        private ChessSquare? _selectedSquare;

        public ChessSquare? SelectedSquare
        {
            get { return _selectedSquare; }
            set
            {
                if (_selectedSquare != value)
                {
                    _selectedSquare = value;
                    OnPropertyChanged(nameof(SelectedSquare));
                }
            }
        }
        private static string? _gameId;
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
        public MainViewModel(LichessApiService lichessApiService)
        {
            _lichessApiService = lichessApiService;
            Chessboard = new List<ChessSquare>();
            Task.Run(async () =>
            {
                try
                {
                    var lichessGame = await _lichessApiService.CreateBotGameAsync();

                    if (lichessGame != null && !string.IsNullOrEmpty(lichessGame.Id))
                    {
                        _gameId = lichessGame.Id;
                        Debug.WriteLine($"Bot game created! Game ID: {_gameId}");

                        _gameStreamingService = new GameStreamingService("lip_JYrY3vNv29oHJWFb4XLN", _gameId, HandleGameUpdate);
                        IsWhite = await _gameStreamingService.GetInitialResponse();
                        Debug.WriteLine(IsWhite ? "You're White!" : "You're Black");
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
            }).GetAwaiter().GetResult();
            Chessboard = GenerateChessboard(IsWhite);
            Task.Run(async () =>
            {
                _gameStreamingService.StartStreamingAsync();
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    OnPropertyChanged(nameof(Chessboard));
                });
            });
        }
        private void HandleGameUpdate(GameUpdate gameUpdate)
        {
            try
            {
                if (gameUpdate != null && gameUpdate.State != null)
                {
                    if (gameUpdate.State.Moves != null)
                    {
                        UpdateChessboard(gameUpdate.State.Moves);
                    }
                    else
                    {
                        Debug.WriteLine("Moves property is null in game update.");
                    }
                }
                else
                {
                    Debug.WriteLine("GameUpdate or State is null in game update.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in HandleGameUpdate: {ex.Message}");
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
                    var fromSquare = Chessboard.FirstOrDefault(square => square.Row == (move[0] - '1') && square.Column == (move[1] - 'a'));
                    var toSquare = Chessboard.FirstOrDefault(square => square.Row == (move[2] - '1') && square.Column == (move[3] - 'a'));
                    if (fromSquare != null && toSquare != null)
                    {
                        string tempPiece = fromSquare.Piece;
                        fromSquare.Piece = toSquare.Piece;
                        toSquare.Piece = tempPiece;
                        fromSquare.UpdatePieceImageSource();
                        toSquare.UpdatePieceImageSource();
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid move format: {move}");
                    }
                }
                else
                {
                    Debug.WriteLine($"Invalid move format: {move}");
                }
            }
            OnPropertyChanged(nameof(Chessboard));
        }
        public async Task MakeMoveAsync(string move)
        {
            try
            {
                await _lichessApiService.MakeMoveAsync(_gameId, move);

                Debug.WriteLine($"Move {move} played successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error making move: {ex.Message}");
            }
        }
        public List<ChessSquare> GenerateChessboard(bool isWhitePlayer)
        {
            for (int row = 0; row < 8; row++)
            {
                List<ChessSquare> rowSquares = new List<ChessSquare>();

                for (int col = 0; col < 8; col++)
                {
                    ChessSquare square = new()
                    {
                        Background = (row + col) % 2 == 0 ? Brushes.White : Brushes.LightGray,
                        Piece = GetInitialPiece(row, col, isWhitePlayer),
                        Row = row,
                        Column = col,
                        SquareName = $"{Convert.ToChar('a' + col)}{(isWhitePlayer ? (8 - row).ToString() : (row + 1).ToString())}",
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
        public bool CanMakeMove { get; private set; }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
