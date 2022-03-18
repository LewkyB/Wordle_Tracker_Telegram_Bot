using Wordle_Tracker_Telegram_Bot.Data;
using Wordle_Tracker_Telegram_Bot.Data.Models;

namespace Wordle_Tracker_Telegram_Bot.Services
{
    public interface IGameService
    {
        Task<DateTime> GetScoreBoardByDateRange();
        Task<GameSummary> ParseGame(ChatMessage chatMessage);
    }

    public class GameService : IGameService
    {
        private readonly ILogger<GameService> _logger;
        private readonly IDatabaseRepository _databaseRepository;
        public GameService(ILogger<GameService> logger, IDatabaseRepository databaseRepository)
        {
            _logger=logger;
            _databaseRepository=databaseRepository;
        }

        public async Task<DateTime> GetScoreBoardByDateRange()
        {
            //
            //TimeSpan week = DateTime.;

            // a week should start on monday and reset on monday

            // a month should be 1st to end of month

            // make look up table of each monday to monday span for each year

            var dateTime = DateTime.Now.StartOfWeek(DayOfWeek.Monday);

            return dateTime;

        }

        public async Task SubmitScore(int game)
        {
            // if better than bestGame
                // update database.bestGame to game

            // if worst than worstGame
                // update

            // save game

        }

        public async Task<bool> IsPlayerAuthorizedToSubmitGame()
        {
            // figure out when nyt resets the wordle word

            // only allow PlayerId to submit single game in 24 hr
            // period, resetting when wordle word does
            return false;
        }

        public async Task<GameSummary> ParseGame(ChatMessage chatMessage)
        {

            if (chatMessage is null)
            {
                throw new ArgumentNullException(nameof(chatMessage));
            }

            // if messageId in system, then ignore
            if (_databaseRepository.IsDuplicateMessage(chatMessage.MessageId))
            {
                _logger.LogInformation($"Duplicate chatMessage -- {nameof(chatMessage.MessageId)} = {chatMessage.MessageId}");
                return new GameSummary();
            }

            // regex to get score
            var score = 0;
            // regex to get attempts
            var attempts = 0;

            // if regex empty return empty game summary

            var gameSummary = new GameSummary
            {
                ChatId = chatMessage.ChatId,
                MessageId = chatMessage.MessageId,
                DatePlayed = chatMessage.Date,
                Score = score,
                Attempts = attempts,
                WordleWord = ""
            };

            return gameSummary;
        }
    }
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
