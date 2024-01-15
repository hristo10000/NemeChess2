using Microsoft.Extensions.Configuration;
using NemeChess2.Exceptions;
using NemeChess2.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NemeChess2
{
    public class GameStreamingService : IDisposable
    {
        private readonly IConfigurationRoot _configuration;
        private readonly string _apiToken;
        private readonly Action<GameStateEvent> _handleGameState;
        private readonly HttpClient _httpClient;
        private readonly string _gameId;
        private Stream _response;
        private bool IsMyTurn { get; set; } 
        public bool IsColorDetermined { get; set; } = false;
        public bool IsWhite { get; set; }
        private string requestUrl = "https://lichess.org/api/board/game/stream/";

        public GameStreamingService(IConfigurationRoot configuration, string gameId, Action<GameStateEvent> handleGameState)
        {
            _configuration = configuration;
            _apiToken = _configuration["Lichess:ApiToken"] ?? throw new ArgumentNullException(nameof(configuration));
            _gameId = gameId ?? throw new ArgumentNullException(nameof(gameId));
            _handleGameState = handleGameState ?? throw new ArgumentNullException(nameof(handleGameState));
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
        }

        public async Task StartStreamingAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _response = await _httpClient.GetStreamAsync($"{requestUrl}{_gameId}", cancellationToken);
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
                    
                    var updateGameState = JsonConvert.DeserializeObject<GameStateEvent>(line);
                    if (!IsMyTurn)
                    {
                        if (updateGameState.Moves != null)
                        {
                            _handleGameState.Invoke(updateGameState);
                        }
                        else
                        {
                            GameUpdate update = IsWhite ? JsonConvert.DeserializeObject<GameUpdateWhite>(line) : JsonConvert.DeserializeObject<GameUpdateBlack>(line);
                            _handleGameState.Invoke(update.State);
                        }
                    }
                    IsMyTurn = !IsMyTurn;
                }
            }
            catch (OperationCanceledException)
            {
                throw new StreamStoppedUnexpectedlyEception("The stream has stopped working unexpectedly!");
            }
            catch
            {
                throw new Exception("Unexpected error with the stream occured!");
            }
        }
        public async Task<bool> GetInitialResponse(CancellationToken cancellationToken = default)
         {
            _response = await _httpClient.GetStreamAsync($"{requestUrl}{_gameId}", cancellationToken);
            using var reader = new StreamReader(_response);
            var line = await reader.ReadLineAsync();
            reader.Close();
            var updateWhite = JsonConvert.DeserializeObject<GameUpdateWhite>(line);
            IsWhite = updateWhite?.White?.Id == _configuration["Lichess:UserId"];
            IsMyTurn = IsWhite;
            return IsWhite;
        }
        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
