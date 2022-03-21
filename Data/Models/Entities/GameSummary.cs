namespace Wordle_Tracker_Telegram_Bot.Data.Models.Entities
{
    public class GameSummary
    {
        public long? GameSummaryId { get; set; }
        public long? ChatId { get; set; }
        public int? MessageId { get; set; }
        public int? PlayerId { get; set; }
        public DateTime? DatePlayed { get; set; }
        public int? Score { get; set; }
        public int? Attempts { get; set; }
    }
}
