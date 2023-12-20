using NemeChess2.Exceptions;
using NemeChess2.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NemeChess2
{
    public class GameStreamingService : IDisposable
    {
        private readonly string _apiToken;
        private readonly Action<GameUpdate> _handleGameUpdate;
        private readonly Action<GameUpdateGameState> _handleGameState;
        private readonly HttpClient _httpClient;
        private readonly string _gameId;
        private Stream _response;
        public bool IsColorDetermined { get; set; } = false;
        public bool IsWhite { get; set; }

        public GameStreamingService(string apiToken, string gameId, Action<GameUpdate> handleGameUpdate, Action<GameUpdateGameState> handleGameState)
        {
            //TODO: read api token from appsettings here
            _apiToken = apiToken ?? throw new ArgumentNullException(nameof(apiToken));
            _gameId = gameId ?? throw new ArgumentNullException(nameof(gameId));
            _handleGameUpdate = handleGameUpdate ?? throw new ArgumentNullException(nameof(handleGameUpdate));
            _handleGameState = handleGameState ?? throw new ArgumentNullException(nameof(handleGameState));
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
        }
        public async Task StartStreamingAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _response = await _httpClient.GetStreamAsync($"https://lichess.org/api/board/game/stream/{_gameId}", cancellationToken);
                if (_response == null) throw new ResponseNullException("There was an issue with initializing the game stream! First call GetInitialResponse()!");
                using var reader = new StreamReader(_response);

                while (!reader.EndOfStream)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var line = await reader.ReadLineAsync();
                    if (line == string.Empty)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    var updateGameState = JsonConvert.DeserializeObject<GameUpdateGameState>(line);
                    if (updateGameState.Moves != null)
                    {
                        _handleGameState.Invoke(updateGameState);
                    }
                    else
                    {
                        GameUpdate update = IsWhite ? JsonConvert.DeserializeObject<GameUpdateWhite>(line) : JsonConvert.DeserializeObject<GameUpdateBlack>(line);//TODO: make an interface for GameUpdate
                        _handleGameUpdate.Invoke(update);//TODO: when the identical classes are merged into one, use _updateGameState
/*                        if (IsWhite)
                        {
                            var updateWhite = JsonConvert.DeserializeObject<GameUpdateWhite>(line);
                            _handleGameUpdate.Invoke(updateWhite);
                        }
                        else
                        {
                            var updateBlack = JsonConvert.DeserializeObject<GameUpdateBlack>(line);
                            _handleGameUpdate.Invoke(updateBlack);
                        }*/
                    }
                }
            }
            catch (OperationCanceledException)//TODO: add custom exception with a meaningful message
            {
                throw;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> GetInitialResponse(CancellationToken cancellationToken = default)
         {
            _response = await _httpClient.GetStreamAsync($"https://lichess.org/api/board/game/stream/{_gameId}", cancellationToken);
            using var reader = new StreamReader(_response);
            var line = await reader.ReadLineAsync();
            reader.Close();
            var updateWhite = JsonConvert.DeserializeObject<GameUpdateWhite>(line);
            IsWhite = updateWhite?.White?.Id == "ico_i";//TODO: user id in appasettings, difficulty also in app settings, low prio
            return IsWhite;
        }
        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
