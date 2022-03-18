using Microsoft.EntityFrameworkCore;
using Wordle_Tracker_Telegram_Bot.Data.Models;

namespace Wordle_Tracker_Telegram_Bot.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }

        public DbSet<ChatMessage>? ChatMessages { get; set; }
        public DbSet<PlayerProfile>? PlayerProfiles { get; set; }
        public DbSet<GameSummary>? GameSummaries { get; set; }
    }
    public interface IDatabaseRepository
    {
        bool IsDuplicateMessage(int? messageId);
    }

    public class DatabaseRepository : IDatabaseRepository
    {
        private readonly ILogger<DatabaseRepository> _logger;
        private readonly DatabaseContext _databaseContext;
        public DatabaseRepository(ILogger<DatabaseRepository> logger, DatabaseContext databaseContext)
        {
            _logger=logger;
            _databaseContext=databaseContext;
        }

        public NotImplementedException GetPlayerScoreByDateRange(int PlayerId, DateTime startDate, DateTime endDate)
        {
            // weekly
            // monthly
            // yearly
            // total
            return new NotImplementedException();
        }

        public NotImplementedException GetChatSummaryByDateRange(int ChatId, DateTime startDate, DateTime endDate)
        {
            // weekly
            // monthly
            // yearly
            // total

            // foreach player with ChatId
            //      print name getScoreByDateRange(playerId, start, end)

            return new NotImplementedException();
        }

        public NotImplementedException SaveGame(int playerId, int chatId, int messageId)
        {


            return new NotImplementedException();
        }

        public bool IsDuplicateMessage(int? messageId)
        {
            if (messageId == null) throw new ArgumentNullException(nameof(messageId));

            if (_databaseContext.ChatMessages.Any(chatMessage => chatMessage.MessageId == messageId))
            {
                return true;
            }

            return false;
        }



    }

}
