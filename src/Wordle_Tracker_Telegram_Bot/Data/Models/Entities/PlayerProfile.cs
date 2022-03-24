using Wordle_Tracker_Telegram_Bot.Data.Models.Entities;

namespace Wordle_Tracker_Telegram_Bot.Data.Models
{
    public class PlayerProfile
    {
        // TODO: which actually need to be nullable?
        public int? PlayerProfileId { get; set; }
        public int? PlayerId { get; set; }
        public int? ChatId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public int? TotalGamesPlayed { get; set; }

        public List<GameSummary>? GameSummaries { get; set; }

        public GameSummary? BestGame { get; set; }
        public GameSummary? WorstGame { get; set; }
    }
}
