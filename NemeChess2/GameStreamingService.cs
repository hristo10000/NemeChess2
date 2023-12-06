using NemeChess2.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
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
                using var response = await _httpClient.GetStreamAsync($"https://lichess.org/api/board/game/stream/{_gameId}", cancellationToken);
                using var reader = new StreamReader(response);

                while (!reader.EndOfStream)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var line = await reader.ReadLineAsync();

                    Debug.WriteLine($"Received JSON data: {line}");

                    try
                    {
                        var gameUpdate = JsonSerializer.Deserialize<GameUpdate>(line);
                        _handleGameUpdate?.Invoke(gameUpdate);
                    }
                    catch (JsonException ex)
                    {
                        Debug.WriteLine($"JsonException: {ex.Message}");
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


        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
