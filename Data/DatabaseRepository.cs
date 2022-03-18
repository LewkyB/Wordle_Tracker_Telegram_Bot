using Microsoft.EntityFrameworkCore;
using Wordle_Tracker_Telegram_Bot.Data.Models;
using Wordle_Tracker_Telegram_Bot.Data.Models.Entities;

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
        IQueryable<GameSummary> GetPlayerGamesByDateRange(int? playerId, DateTime startDateTime, DateTime endDateTime);
        IQueryable<PlayerProfile> GetPlayersByChatId(long? chatId);
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

        public IQueryable<PlayerProfile> GetPlayersByChatId(long? chatId)
        {
            return _databaseContext.PlayerProfiles
                .Where(profile => profile.ChatId == chatId);
        }

        public IQueryable<GameSummary> GetPlayerGamesByDateRange(int? playerId, DateTime startDateTime, DateTime endDateTime)
        {
            return _databaseContext.GameSummaries
                .Where(game => game.PlayerId == playerId)
                .Where(game => game.DatePlayed > startDateTime && game.DatePlayed <= endDateTime);
        }
    }

}
