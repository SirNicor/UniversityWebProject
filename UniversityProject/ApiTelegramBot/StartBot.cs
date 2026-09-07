using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.Json;
using System.Text.Encodings;
namespace ApiTelegramBot;
using Logger;
using Telegram.Bot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Logger;
using Repository;
using UCore;
public class StartBot : IStartBot
{
    private TelegramBotClient _botClient;
    private string _token;
    private ReceiverOptions _receiverOptions;
    private MyLogger _logger;
    private FunctionOfBot _functionOfBot;
    public StartBot(IGetToken getToken, MyLogger logger, FunctionOfBot functionOfBot)
    {
        _token = getToken.ReturnToken();
        _logger = logger;
        _functionOfBot = functionOfBot;
    }
    public async Task ListenForMessagesAsync()
    {
        _botClient = new TelegramBotClient(_token);
        using var cts = new CancellationTokenSource();

        _receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>(),
            DropPendingUpdates = true
        };
        var me = await _botClient.GetMe();
        _logger.Info($"{me.FirstName} запущен!", "ApiTelegramBot:StartBot");
        await _botClient.ReceiveAsync(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandlePollingErrorAsync,
            receiverOptions: _receiverOptions,
            cancellationToken: cts.Token
        );;
        Console.ReadKey();
        cts.Cancel();
    }
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message || message.Text is not { } messageText)
        {
            return;
        }
        var id = message.Chat.Id;
        var type = message.Chat.Type;
        var userStateReg = _functionOfBot.UserStateTelegramRepository.Get(id);
        if (userStateReg == null)
        {
            userStateReg = new UserStateRegistration(id);
            _functionOfBot.UserStateTelegramRepository.Create(userStateReg);
        }
        string loggerInfo = $"ChatId: {id}, UserName:{message.Chat.Username}, FirstName: {message.Chat.FirstName}, LastName: {message.Chat.LastName}," +
                            $" ChatType: {type}, MessageText: {messageText}, Photo: {message.Photo}" +
                            $", Audio: {message.Audio},  Video: {message.Video}" +
                            $"UserStateRequest: {userStateReg.RequestType}";
        _logger.Info(loggerInfo, "ApiTelegramBot:StartBot");
        IRegistrationClass registrationClass = null;
        switch (type)
        {
            case ChatType.Private:
            {
                switch (userStateReg.UserState)
                {
                    case UserStateRegEnum.notInitialized:
                    {
                        await _functionOfBot.InitializedClass.Initialize(id, botClient, messageText, userStateReg);
                        return;
                    }
                    case UserStateRegEnum.forRegistration:
                    {
                        await _functionOfBot.RegistrationClass.Registration(id, botClient, messageText, userStateReg);
                        return;
                    }
                    case UserStateRegEnum.waitingForUniversityInput:
                    {   
                        await _functionOfBot.RegistrationForUniversity.Registration(id, botClient, messageText, userStateReg);
                        return;
                    }
                    case UserStateRegEnum.waitingForFacultyInput:
                    {
                        await _functionOfBot.RegistrationForFaculty.Registration(id, botClient, messageText, userStateReg);
                        return;
                    }
                    case UserStateRegEnum.waitingForDepartmentInput:
                    {
                        await _functionOfBot.RegistrationForDepartment.Registration(id, botClient, messageText, userStateReg);
                        return;
                    }
                    case UserStateRegEnum.waitingForDirectionInput:
                    {
                        await _functionOfBot.RegistrationForDirection.Registration(id, botClient, messageText, userStateReg);
                        return;
                    }
                    case UserStateRegEnum.waitingForLastNameInput:
                    {
                        await _functionOfBot.RegistrationForLastName.Registration(id, botClient, messageText, userStateReg);
                        return;
                    }
                    case UserStateRegEnum.waitingForFirstNameInput:
                    {
                        await _functionOfBot.RegistrationForFirstName.Registration(id, botClient, messageText, userStateReg);
                        return;
                    }
                    case UserStateRegEnum.fullRegistration:
                    {
                        await _functionOfBot.StartFunctionalForGroup.Functional(id, messageText, botClient, ChatType.Private, userStateReg);
                        return;
                    }
                }
                return;
            }
            case ChatType.Group:
            {
                await _functionOfBot.StartFunctionalForGroup.Functional(id, messageText, botClient, ChatType.Group, userStateReg);
                return;
            }
        }
    }
    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}