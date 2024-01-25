using Microsoft.Extensions.Configuration;
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
        _apiToken = _configuration["Lichess:ApiToken"];

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://lichess.org/api/")
        };
    }
    public async Task<LichessGame> CreateBotGameAsync()
    {
        try
        {
            int level = int.Parse(_configuration["Lichess:AiDifficulty"]);
            int clockLimit = int.Parse(_configuration["Lichess:ClockLimit"]);
            int clockIncrement = int.Parse(_configuration["Lichess:ClockIncrement"]);
            int days = int.Parse(_configuration["Lichess:Days"]);
            string color = _configuration["Lichess:Color"];
            string variant = _configuration["Lichess:Variant"];
            string fen = _configuration["Lichess:Fen"];

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
                    Debug.WriteLine($"Failed to challenge AI. Status code: {response.StatusCode}");
                    return null;
                }
            }
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"IOException: {ex.Message}");
            Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            Debug.WriteLine($"InnerException: {ex.InnerException?.Message}");

            throw;
        }
        catch{
            throw;
        }
    }

    public async Task<bool> MakeMoveAsync(string gameId, string move)
    {
        try
        {
            var url = $"/api/board/game/{gameId}/move/{move}";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", $"Bearer {_apiToken}");

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Successfully made move. Status code: {response.StatusCode}");
            if(response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return false;
            }
            return true;
        }
        catch (HttpRequestException ex)
        { 
            Debug.WriteLine($"Error making move: {ex.Message}");
            return false;
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
            Debug.WriteLine($"Error in game stream: {ex.Message}");
        }
    }
}
