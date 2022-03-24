using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Wordle_Tracker_Telegram_Bot.Data.Models;
using Wordle_Tracker_Telegram_Bot.Data.Models.Entities;

namespace Wordle_Tracker_Telegram_Bot.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<PlayerProfile>? PlayerProfiles { get; set; }
        public DbSet<GameSummary>? GameSummaries { get; set; }
    }

    public interface IDatabaseRepository
    {
        Task<bool> CheckIfSenderIsInDatabase(Message message);
        IQueryable<GameSummary> GetPlayerGamesByDateRange(int? playerId, DateTime startDateTime, DateTime endDateTime);
        IQueryable<PlayerProfile> GetPlayersByChatId(long? chatId);
        bool IsDuplicateGame(int? messageId);
        Task SaveGame(GameSummary gameSummary);
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

        // TODO: broken
        public bool IsDuplicateGame(int? messageId)
        {
            if (messageId == null) throw new ArgumentNullException(nameof(messageId));

            if (_databaseContext.GameSummaries.Any(game => game.MessageId == messageId))
            {
                // duplicate game found
                return true;
            }

            // no duplicate game found
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

        public async Task SaveGame(GameSummary gameSummary)
        {
            try
            {
                // if better than bestGame
                    // update database.bestGame to game

                // if worst than worstGame
                    // update

                // save game
                await _databaseContext.GameSummaries.AddAsync(gameSummary);
                await _databaseContext.SaveChangesAsync();

            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException();
            }
            catch (DbUpdateException)
            {
                throw new DbUpdateException();
            }
        }
        public async Task<bool> CheckIfSenderIsInDatabase(Message message)
        {
            bool playerProfilePresent = await _databaseContext.PlayerProfiles.AnyAsync(playerProfile => playerProfile.PlayerId == message.From.Id);

            if (!playerProfilePresent)
            {
                // add player to database
            }
            return false;
        }
    }

}
