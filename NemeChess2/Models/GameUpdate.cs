namespace NemeChess2.Models
{
    public class GameUpdate
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public Variant Variant { get; set; }
        public Clock Clock { get; set; }
        public string Speed { get; set; }
        public Perf Perf { get; set; }
        public long CreatedAt { get; set; }
        public GameEventPlayer White { get; set; }
        public GameEventPlayer Black { get; set; }
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
        public bool Provisional { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; }
    }

    public class GameStateEvent
    {
        public string Type { get; set; }
        public string Moves { get; set; }
        public long Wtime { get; set; }
        public long Btime { get; set; }
        public int Winc { get; set; }
        public int Binc { get; set; }
        public string Status { get; set; }
    }
}
