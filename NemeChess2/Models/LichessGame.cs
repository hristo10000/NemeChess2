namespace NemeChess2.Models
{
    public class LichessGame
    {
        public string Id { get; set; }
        public Variant Variant { get; set; }
        public string Speed { get; set; }
        public string Perf { get; set; }
        public bool Rated { get; set; }
        public string Fen { get; set; }
        public int Turns { get; set; }
        public string Source { get; set; }
        public Status Status { get; set; }
        public long CreatedAt { get; set; }
        public string Player { get; set; }
    }

    public class Variant
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Short { get; set; }
    }

    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
