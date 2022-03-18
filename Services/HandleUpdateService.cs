using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Wordle_Tracker_Telegram_Bot.Data.Models;

namespace Wordle_Tracker_Telegram_Bot.Services;

public class HandleUpdateService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;
    private readonly IGameService _gameService;

    public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger, IGameService gameService)
    {
        _botClient = botClient;
        _logger = logger;
        _gameService=gameService;
    }

    public async Task EchoAsync(Update update)
    {
        var handler = update.Type switch
        {
            // UpdateType.Unknown:
             //UpdateType.ChannelPost => AcknowledgeMessage(update),
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.Message => BotOnMessageReceived(update.Message!),
            UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage!),
            UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery!),
            UpdateType.InlineQuery => BotOnInlineQueryReceived(update.InlineQuery!),
            UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult!),
            _ => UnknownUpdateHandlerAsync(update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    private async Task BotOnMessageReceived(Message message)
    {
        _logger.LogInformation($"Receive message type: {message.Type}");
        if (message.Type != MessageType.Text)
            return;

        var action = message.Text!.Split(' ')[0] switch
        {
            "/inline" => SendInlineKeyboard(_botClient, message),
            "/keyboard" => SendReplyKeyboard(_botClient, message),
            "/remove" => RemoveKeyboard(_botClient, message),
            "/photo" => SendFile(_botClient, message),
            "/webhookinfo" => GetWebhookInfo(_botClient, message),
            "/request" => RequestContactAndLocation(_botClient, message),
            _ => Usage(_gameService, _botClient, message)
        };
        Message sentMessage = await action;
        _logger.LogInformation($"The message was sent with id: {sentMessage.MessageId}");

        // Send inline keyboard
        // You can process responses in BotOnCallbackQueryReceived handler
        static async Task<Message> SendInlineKeyboard(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            // Simulate longer running task
            await Task.Delay(500);

            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", "11"),
                        InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    },
                });

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: "Choose",
                                                  replyMarkup: inlineKeyboard);
        }

        static async Task<Message> SendReplyKeyboard(ITelegramBotClient bot, Message message)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                        new KeyboardButton[] { "1.1", "1.2" },
                        new KeyboardButton[] { "2.1", "2.2" },
                })
            {
                ResizeKeyboard = true
            };

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: "Choose",
                                                  replyMarkup: replyKeyboardMarkup);
        }

        static async Task<Message> RemoveKeyboard(ITelegramBotClient bot, Message message)
        {
            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: "Removing keyboard",
                                                  replyMarkup: new ReplyKeyboardRemove());
        }

        static async Task<Message> SendFile(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            const string filePath = @"Files/tux.png";
            using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

            return await bot.SendPhotoAsync(chatId: message.Chat.Id,
                                            photo: new InputOnlineFile(fileStream, fileName),
                                            caption: "Nice Picture");
        }

        static async Task<Message> RequestContactAndLocation(ITelegramBotClient bot, Message message)
        {
            ReplyKeyboardMarkup RequestReplyKeyboard = new(
                new[]
                {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact"),
                });

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: "Who or Where are you?",
                                                  replyMarkup: RequestReplyKeyboard);
        }

        static async Task<Message> Usage(IGameService gameService, ITelegramBotClient bot, Message message)
        {
            const string usage = "Usage:\n" +
                                 "/inline       - send inline keyboard\n" +
                                 "/keyboard     - send custom keyboard\n" +
                                 "/weekly       - get this week's scores\n" +
                                 "/photo        - send a photo\n" +
                                 "/webhookinfo  - get uh webhook info\n" +
                                 "/request      - request location or contact";

            if (message is null) new ArgumentNullException(nameof(message));

            var chatMessage = new ChatMessage
            {
                ChatId = message.Chat.Id,
                MessageId = message.MessageId,
                Text = message.Text,
                Date = message.Date,
                SenderId = message.From.Id,
                SenderFirstName = message.From.FirstName,
                SenderLastName = message.From.LastName,
                SenderUserName = message.From.Username,
                IsSenderBot = message.From.IsBot
            };

            var week = await gameService.GetScoreBoardByDateRange();

            // process the text
            // not implemented
            _ = await gameService.ParseGame(chatMessage);

            // get the score
            // how to track tiles: blank, yellow, green

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: $"message.Text:\t{message.Text}\n" +
                                                  $"message.MessageId:\t{message.MessageId}\n" +
                                                  $"message.From:\t{message.From}\n" +
                                                  $"message.Date:\t{message.Date}\n" +
                                                  $"message.Chat.Id:\t{message.Chat.Id}\n" +
                                                  $"Scoreboard Date:\t{week}",
                                                  replyMarkup: new ReplyKeyboardRemove());
        }

        static async Task<Message> GetWebhookInfo(ITelegramBotClient bot, Message message)
        {
            var webHookInfo = await bot.GetWebhookInfoAsync();
            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: $"URL: {webHookInfo.Url}\n" +
                                                  $"Max Connections: {webHookInfo.MaxConnections}\n" +
                                                  $"HasCustomCertificate: {webHookInfo.HasCustomCertificate}\n" +
                                                  $"IP Address: {webHookInfo.IpAddress}\n" +
                                                  $"Last Error Date: {webHookInfo.LastErrorDate}\n" +
                                                  $"Last Error Message: {webHookInfo.LastErrorMessage}\n" +
                                                  $"AllowedUpdates: {webHookInfo.AllowedUpdates}\n" +
                                                  $"PendingUpdateCount: {webHookInfo.PendingUpdateCount}");
        }
    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
    {
        await _botClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"Received {callbackQuery.Data}");

        await _botClient.SendTextMessageAsync(
            chatId: callbackQuery.Message.Chat.Id,
            text: $"Received {callbackQuery.Data}");
    }

    #region Inline Mode

    private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery)
    {
        _logger.LogInformation($"Received inline query from: {inlineQuery.From.Id}");

        InlineQueryResult[] results = {
            // displayed result
            new InlineQueryResultArticle(
                id: "3",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent(
                    "hello"
                )
            )
        };

        await _botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id,
                                                results: results,
                                                isPersonal: true,
                                                cacheTime: 0);
    }

    private Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult)
    {
        _logger.LogInformation($"Received inline result: {chosenInlineResult.ResultId}");
        return Task.CompletedTask;
    }

    #endregion

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }

    public Task HandleErrorAsync(Exception exception)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation($"HandleError: {ErrorMessage}");
        return Task.CompletedTask;
    }
}
