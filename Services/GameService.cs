using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Wordle_Tracker_Telegram_Bot.Data;
using Wordle_Tracker_Telegram_Bot.Data.Models;
using Wordle_Tracker_Telegram_Bot.Data.Models.Entities;

namespace Wordle_Tracker_Telegram_Bot.Services
{
    public interface IGameService
    {
        Task CheckMessageForGame(Message message);
        Task<IEnumerable<ScoreboardRow>> GetScoreBoardByDateRange(Message message);
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

        public async Task<IEnumerable<ScoreboardRow>> GetScoreBoardByDateRange(Message message)
        {
            // get chatId
            // get all PlayerIds with chatId
            var players = _databaseRepository.GetPlayersByChatId(message.Chat.Id);

            List<ScoreboardRow> scores = new List<ScoreboardRow>();

            // for each PlayerId
            if (players != null) return scores;

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

        public async Task CheckMessageForGame(Message message)
        {
            if (_databaseRepository.IsDuplicateGame(message.MessageId)) return;
            if (message.Text is null) return;

            await CheckIfSenderIsInDatabase(message.From.Id);

            string wordleScoreShareRegex = @"(\w+) (\w+)(?: (\w+))";
            Regex regex = new(wordleScoreShareRegex, RegexOptions.IgnoreCase);

            MatchCollection? matches = regex.Matches(message.Text);

            if (
                matches[0].Groups[1].Value == "Wordle" &&
                int.TryParse(matches[0].Groups[2].Value, out int gameScore) && // does this work how I think?
                int.TryParse(matches[0].Groups[3].Value, out int attempts)
                )
            {
                GameSummary gameSummary = new GameSummary()
                {
                    ChatId = message?.Chat.Id ?? 0,
                    DatePlayed = message?.Date ?? DateTime.Now,
                    MessageId = message?.MessageId ?? 0,
                    PlayerId = (int?)(message?.From?.Id ?? 0),
                    Attempts = int.TryParse(matches[0].Groups[3].Value, out var attempt) ? attempt : 0, // does this work the way I think it does?
                    Score = int.TryParse(matches[0].Groups[2].Value, out var score) ? score : 0,
                };

                await _databaseRepository.SaveGame(gameSummary);

                // return message saying that you received player's score and recorded it
            }
        }

        public async Task CheckIfSenderIsInDatabase(long id)
        {
            if(!await _databaseRepository.CheckIfSenderIsInDatabase(id))
            {
                // logic to add sender to PlayerProfile
            }
        }

        public async Task<bool> IsPlayerAuthorizedToSubmitGame()
        {
            // figure out when nyt resets the wordle word

            // only allow PlayerId to submit single game in 24 hr
            // period, resetting when wordle word does
            return false;
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
