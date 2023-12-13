using Microsoft.Extensions.Configuration;
using NemeChess2;
using NemeChess2.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class LichessApiService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _apiToken;
    public LichessApiService(IConfiguration configuration)
    {
        _configuration = configuration;
        _httpClient = new HttpClient();
        _apiToken = _configuration["Lichess:ApiToken"];
    }
    public LichessApiService(string apiToken)
    {
        _apiToken = apiToken;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://lichess.org/api/");
    }
    public async Task<LichessGame> CreateBotGameAsync()
    {
        try
        {
            int level = 5;
            int clockLimit = 10800;
            int clockIncrement = 5;
            int days = 1;
            string color = "random";
            string variant = "standard";
            string fen = "";

            var requestBody = new StringBuilder();
            requestBody.Append($"level={level}");
            requestBody.Append($"&clock.limit={clockLimit}");
            requestBody.Append($"&clock.increment={clockIncrement}");
            requestBody.Append($"&days={days}");
            requestBody.Append($"&color={color}");
            requestBody.Append($"&variant={variant}");
            if (!string.IsNullOrEmpty(fen))
            {
                requestBody.Append($"&fen={fen}");
            }
            var requestContent = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");
            using (var request = new HttpRequestMessage(HttpMethod.Post, "https://lichess.org/api/challenge/ai"))
            {
                request.Headers.Add("Authorization", $"Bearer {_apiToken}");
                /*
                 TODO: 
                1: Send Request to create game. 
                1.5: get game id
                2: DetermineGameType(gameID) method: Try Casting to white game update, if deserialization is successful - continue, otherwise - try casting to black
                */
                request.Content = requestContent;

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var lichessGame = JsonSerializer.Deserialize<LichessGame>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        return lichessGame;
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {content}");

                    Console.WriteLine($"Failed to challenge AI. Status code: {response.StatusCode}");
                    Console.WriteLine($"Response Content: {content}");
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return null;
        }
    }
    public async Task MakeMoveAsync(string gameId, string move, bool offeringDraw = false)
    {
        try
        {
            var url = $"/api/board/game/{gameId}/move/{move}";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", $"Bearer {_apiToken}");

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            Console.WriteLine("Move successfully made.");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error making move: {ex.Message}");
            throw;
        }
    }
    public async Task<LichessGame> GetChessboardState(string gameId)
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"https://lichess.org/api/game/{gameId}");

            if (!string.IsNullOrEmpty(response))
            {
                var lichessGame = JsonSerializer.Deserialize<LichessGame>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return lichessGame;
            }
            else
            {
                Console.WriteLine("Failed to retrieve game state. Check the console for details.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return null;
        }
    }
    public async Task StartGameStream(string gameId, Action<GameUpdate> handleGameUpdate)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");

            using var response = await client.GetStreamAsync($"https://lichess.org/api/board/game/stream/{gameId}");
            using var reader = new StreamReader(response);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var gameUpdate = JsonSerializer.Deserialize<GameUpdate>(line);
                handleGameUpdate?.Invoke(gameUpdate);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in game stream: {ex.Message}");
        }
    }
    private void HandleGameUpdate(GameUpdate gameUpdate)
    {
        switch (gameUpdate.Type)
        {
            case "gameFull":
                HandleFullGameData(gameUpdate);
                break;
            case "gameState":
                HandleGameState(gameUpdate);
                break;
            case "chatLine":
                HandleChatMessage(gameUpdate);
                break;
            case "opponentGone":
                HandleOpponentGone(gameUpdate);
                break;
        }
    }

    private void HandleFullGameData(GameUpdate gameUpdate)
    {
        Debug.WriteLine("Received full game data");
    }

    private void HandleGameState(GameUpdate gameUpdate)
    {
        Debug.WriteLine("Received game state update");
    }

    private void HandleChatMessage(GameUpdate gameUpdate)
    {
        Debug.WriteLine("Received chat message");
    }

    private void HandleOpponentGone(GameUpdate gameUpdate)
    {
        Debug.WriteLine("Opponent has left the game");
    }

}
