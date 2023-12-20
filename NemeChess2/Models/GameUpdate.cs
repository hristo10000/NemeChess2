namespace NemeChess2.Models
{
    public class GameUpdateGameState //TODO: remove, use game state event instead
    {
        public string Moves { get; set; }
        public int Wtime { get; set; }
        public int Btime { get; set; }
        public int Winc { get; set; }
        public int Binc { get; set; }
        public string Status { get; set; }
    }
    public class GameUpdate
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public bool Rated { get; set; }
        public Variant Variant { get; set; }
        public Clock Clock { get; set; }
        public string Speed { get; set; }
        public Perf Perf { get; set; }
        public long CreatedAt { get; set; }
        public string InitialFen { get; set; }
        public GameStateEvent State { get; set; }
    }
    public class Clock
    {
        public long Initial { get; set; }
        public long Increment { get; set; }
    }
    public class Perf
    {
        public string Name { get; set; }
    }
    public class GameEventPlayer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Title { get; set; }
        public long Rating { get; set; }
        public bool Provisional { get; set; }
    }
    public class GameStateEvent
    {
        public string? Type { get; set; }
        public string Moves { get; set; }
        public long Wtime { get; set; }
        public long Btime { get; set; }
        public int Winc { get; set; }
        public int Binc { get; set; }
        public string Status { get; set; }
    }
    public class GameUpdateWhite : GameUpdate
    {
        public GameEventPlayer White { get; set; }
        public AiLevelType Black { get; set; }
    }
    public class GameUpdateBlack : GameUpdate
    {
        public GameEventPlayer Black { get; set; }
        public AiLevelType White { get; set; }

    }
    public class AiLevelType
    {
        public int AiLevel { get; set; }
    }
}
