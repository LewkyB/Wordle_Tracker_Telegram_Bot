namespace Wordle_Tracker_Telegram_Bot.Data.Models
{
    public class ChatMessage
    {
        public long? ChatId { get; set; }
        public int? MessageId { get; set; }
        public string? Text { get; set; }
        public DateTime? Date { get; set; }
        public long? SenderId { get; set; }
        public string? SenderFirstName { get; set; }
        public string? SenderLastName { get; set; }
        public string? SenderUserName { get; set; }
        public bool? IsSenderBot { get; set; }
    }
}
