using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

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
            // UpdateType.ChannelPost:
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
            "/webhookinfo" => GetWebhookInfo(_botClient, message, _gameService),
            "/score" => GetWebhookInfo(_botClient, message, _gameService),
            _ => ProcessMessage(_gameService, _botClient, message, _logger)
        };

        Message sentMessage = await action;

        _logger.LogInformation($"The message was sent with id: {sentMessage.MessageId}");

        // passing the logger like this is gross
        static async Task<Message> ProcessMessage(IGameService gameService, ITelegramBotClient bot, Message message, ILogger<HandleUpdateService> logger)
        {
            const string usage = "Usage:\n" +
                                 "/score       - get this week's scores\n" +
                                 "/webhookinfo  - get uh webhook info\n";

            if (message is null)
            {
                logger.LogInformation($"{nameof(message)} is null");
                return new Message();
            }

            await gameService.CheckMessageForGame(message);

            var week = await gameService.GetScoreBoardByDateRange(message);

            // get the score
            // how to track tiles: blank, yellow, green

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: $"message.Text:\t{message.Text}\n" +
                                                  $"message.MessageId:\t{message.MessageId}\n" +
                                                  $"message.From:\t{message.From}\n" +
                                                  $"message.Date:\t{message.Date}\n" +
                                                  $"message.Chat.Id:\t{message.Chat.Id}\n" +
                                                  $"Scoreboard Date:\t",
                                                  //$"Scoreboard Date:\t{week}",
                                                  replyMarkup: new ReplyKeyboardRemove());
        }

        static async Task<Message> GetWebhookInfo(ITelegramBotClient bot, Message message, IGameService gameService)
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
