using Wordle_Tracker_Telegram_Bot.Data;
using Wordle_Tracker_Telegram_Bot.Data.Models;
using Wordle_Tracker_Telegram_Bot.Data.Models.Entities;

namespace Wordle_Tracker_Telegram_Bot.Services
{
    public interface IGameService
    {
        Task<IEnumerable<ScoreboardRow>> GetScoreBoardByDateRange(ChatMessage chatMessage);
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

        public async Task<IEnumerable<ScoreboardRow>> GetScoreBoardByDateRange(ChatMessage chatMessage)
        {
            // get chatId
            // get all PlayerIds with chatId
            var players = _databaseRepository.GetPlayersByChatId(chatMessage.ChatId);

            List<ScoreboardRow> scores = new List<ScoreboardRow>();

            // for each PlayerId
            foreach (var player in players)
            {
                // get all the games between now and {DateTime.Now.StartOfWeek(DayOfWeek.Monday)}
                var weekStart = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
                var now = DateTime.Now;

                var playerGames = _databaseRepository.GetPlayerGamesByDateRange(player.PlayerId, startDateTime: weekStart, endDateTime: now);

                var gamesPlayed = playerGames.Count();

                // add up the scores of all of the games
                var playerWeeklyTotalScore = playerGames.Sum(game => game.Score);

                var averageScore = playerWeeklyTotalScore / gamesPlayed;

                var playerScoreboardRow = new ScoreboardRow
                {
                    Name = player.FirstName,
                    Score = playerWeeklyTotalScore,
                    GamesPlayed = gamesPlayed,
                    AverageScore = averageScore,
                };

                scores.Add(playerScoreboardRow);
            }

            return scores.OrderByDescending(s => s.Score);
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
                _logger.LogError($"{nameof(chatMessage)} is null.");
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
