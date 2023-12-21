using NemeChess2.Models;
using NemeChess2.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Diagnostics;
using DynamicData;
using Avalonia.Media;
using Microsoft.Extensions.Configuration;

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
        public bool IsMyTurn { get; set; }
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
        private Dictionary<string, ChessSquare> _chessboardDictionary = new Dictionary<string, ChessSquare>();
        private readonly IConfigurationRoot _configuration;
        public MainViewModel(LichessApiService lichessApiService, IConfigurationRoot configuration)
        {
            _lichessApiService = lichessApiService;
            _configuration = configuration;
            Chessboard = new List<ChessSquare>();
            Task.Run(async () =>
            {
                try
                {
                    var lichessGame = await _lichessApiService.CreateBotGameAsync();

                    if (lichessGame != null && !string.IsNullOrEmpty(lichessGame.Id))
                    {
                        _gameId = lichessGame.Id;
                        _gameStreamingService = new GameStreamingService(_configuration, _gameId, HandleGameState);
                        IsWhite = await _gameStreamingService.GetInitialResponse();
                        IsMyTurn = IsWhite;
                    }
                    else
                    {
                        Debug.WriteLine("Failed to create a bot game.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"An error occurred: {ex.Message}");
                }
            }).GetAwaiter().GetResult();
            Chessboard = GenerateChessboard(IsWhite);
            Task.Run(() =>
            {
                _gameStreamingService.StartStreamingAsync();
            });
        }
        private void HandleGameState(GameStateEvent gameState)
        {
            var moveList = gameState.Moves.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            IsMyTurn = !IsMyTurn;
            if (moveList.Length > 0)
            {
                if (gameState.Status == "mate")
                {
                    Debug.WriteLine(IsMyTurn ? "You Win!" : "You Loose!");//TODO: Add a poppup, test if it works properly
                }
                var lastMove = moveList[moveList.Length - 1];
                UpdateChessboard(lastMove);
            }
        }
        public void UpdateChessboard(string lastMove)
        {
            try
            {
                var fromSquareName = lastMove.Substring(0, 2);
                var toSquareName = lastMove.Substring(2, 2);

                var fromSquare = _chessboardDictionary[fromSquareName];
                var toSquare = _chessboardDictionary[toSquareName];

                toSquare.Piece = fromSquare.Piece;
                fromSquare.Piece = "";

                OnPropertyChanged(nameof(Chessboard));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Chessboard didn't update properly! {ex.Message}");
            }
        }
        public async Task MakeMoveAsync(string move)
        {
            try
            {
                await _lichessApiService.MakeMoveAsync(_gameId, move);
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
                    Chessboard.Add(square);
                    _chessboardDictionary[square.SquareName] = square;
                }
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
