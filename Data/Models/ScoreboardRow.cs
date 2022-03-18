namespace Wordle_Tracker_Telegram_Bot.Data.Models
{
    public class ScoreboardRow
    {
            public string? Name { get; set; }
            public int? Score { get; set; }
            public int? AverageScore { get; set; }
            public int? GamesPlayed { get; set; }
    }
}
