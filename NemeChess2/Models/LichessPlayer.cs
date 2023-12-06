namespace NemeChess2.Models
{
    public class LichessPlayer
    {
        public LichessUser User { get; set; }
        public int Rating { get; set; }
        public int RatingDiff { get; set; }
    }
}
