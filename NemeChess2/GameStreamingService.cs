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
        private readonly HttpClient _httpClient;
        private readonly string _gameId;
        private Stream _response;
        public GameUpdateBlack UpdateBlack { get; set; }
        public GameUpdateWhite UpdateWhite { get; set; }
        public bool IsColorDetermined { get; set; } = false;
        public bool IsWhite { get; set; }

        public GameStreamingService(string apiToken, string gameId, Action<GameUpdate> handleGameUpdate)
        {
            _apiToken = apiToken ?? throw new ArgumentNullException(nameof(apiToken));
            _gameId = gameId ?? throw new ArgumentNullException(nameof(gameId));
            _handleGameUpdate = handleGameUpdate ?? throw new ArgumentNullException(nameof(handleGameUpdate));

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
        }
        public async Task StartStreamingAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_response == null) throw new ResponseNullException("There was an issue with initializing the game stream! First call GetInitialResponse()!");
                using var reader = new StreamReader(_response);

                while (!reader.EndOfStream)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var line = await reader.ReadLineAsync();

                    if (line == string.Empty)
                    {
                        Task.Delay(1000).Wait();
                        continue;
                    }
                    if (IsWhite)
                    {
                        UpdateWhite = JsonConvert.DeserializeObject<GameUpdateWhite>(line);
                        _handleGameUpdate.Invoke(UpdateWhite);
                    }
                    else
                    {
                        UpdateBlack = JsonConvert.DeserializeObject<GameUpdateBlack>(line);
                        _handleGameUpdate.Invoke(UpdateBlack);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in game stream: {ex.Message}");
            }
        }
        public async Task<bool> GetInitialResponse(CancellationToken cancellationToken = default)
        {
            using var _response = await _httpClient.GetStreamAsync($"https://lichess.org/api/board/game/stream/{_gameId}", cancellationToken);
            using var reader = new StreamReader(_response);
            var line = await reader.ReadLineAsync();
            UpdateWhite = JsonConvert.DeserializeObject<GameUpdateWhite>(line);
            return UpdateWhite?.White?.Id == "ico_i";
        }
        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}