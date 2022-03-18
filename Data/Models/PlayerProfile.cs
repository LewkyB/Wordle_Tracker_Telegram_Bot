namespace Wordle_Tracker_Telegram_Bot.Data.Models
{
    public class PlayerProfile
    {
        public int? PlayerId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public int? TotalGamesPlayed { get; set; }

        public List<GameSummary> GameSummaries { get; set; }

        public GameSummary BestGame { get; set; }
        public GameSummary WorstGame { get; set; }
    }
}
