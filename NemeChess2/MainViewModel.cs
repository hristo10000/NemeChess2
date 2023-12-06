using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using System;
using System.Reactive.Linq;

public class MainViewModel : ReactiveObject
{
    private readonly LichessApiService _lichessApiService;
    private string _gameId;
    public MainViewModel(LichessApiService lichessApiService)
    {
        _lichessApiService = lichessApiService;

        CreateBotGameCommand = ReactiveCommand.CreateFromTask(CreateBotGameAsync);
        MakeMoveCommand = ReactiveCommand.CreateFromTask<string>(MakeMoveAsync);

        MakeMoveCommand.CanExecute
            .Select(canExecute => canExecute && !string.IsNullOrEmpty(_gameId))
            .ObserveOn(RxApp.MainThreadScheduler)
            .BindTo(this, x => x.CanMakeMove);
    }
    public ReactiveCommand<Unit, Unit> CreateBotGameCommand { get; }
    public ReactiveCommand<string, Unit> MakeMoveCommand { get; }
    public bool CanMakeMove { get; private set; }
    private async Task CreateBotGameAsync()
    {
        _gameId = (await _lichessApiService.CreateBotGameAsync().ConfigureAwait(false)).Id;
        Console.WriteLine($"Bot game created! Game ID: {_gameId}");
    }
    private async Task MakeMoveAsync(string move)
    {
        await _lichessApiService.MakeMoveAsync(_gameId, move);
        Console.WriteLine($"Move {move} played successfully.");
    }
}
