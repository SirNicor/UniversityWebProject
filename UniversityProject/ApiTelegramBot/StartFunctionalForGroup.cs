using Microsoft.Extensions.Logging.Configuration;

namespace ApiTelegramBot;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Repository;
using UCore;
using Logger;
using IRepositoryAll;
public class StartFunctionalForGroup : IStartFunctionalForGroup
{
    private IDisciplineUpdate _disSend;
    private IStudentUpdate _studentUpdate;
    private IScheduleUpdate _scheduleUpdate;
    private IDirectionRepository _directionRepository;
    private IUserStateTelegramRepository _userStateTelegramRepository;
    private MyLogger _logger;
    private long dirId;

    private async void CreateMenuFunctional(ChatId id, ITelegramBotClient botClient)
    {
        var replyKeyboard = new ReplyKeyboardMarkup(
            new List<KeyboardButton[]>()
            {
                new KeyboardButton[] { ("Список дисциплин"), "Список студентов", },
                new KeyboardButton[] { "Расписание" },
            })
        {
            ResizeKeyboard = true,
        };

        await botClient.SendMessage(
            id,
            "Функционал:",
            replyMarkup: replyKeyboard);
    }
    public StartFunctionalForGroup(IStudentRepository studentRepository, IDirectionRepository directionRepository, 
        IDisciplineUpdate disSend, IStudentUpdate studentUpdate, IUserStateTelegramRepository userStateTelegramRepository,
        IScheduleUpdate scheduleUpdate, MyLogger logger)
    {
        _studentUpdate = studentUpdate;
        _disSend = disSend;
        _scheduleUpdate = scheduleUpdate;
        _directionRepository = directionRepository;
        _userStateTelegramRepository = userStateTelegramRepository;
        _logger = logger;
    }
    public async Task Functional(ChatId id, string messageText, ITelegramBotClient botClient, ChatType type,  UserStateRegistration userState)
    {
        _logger.Info($"Start Functional, chatId: {id}, messageText: {messageText},  type: {type}" +
                     $"UserState: {userState.RequestType}", "ApiTelegramBot:StartFunctionalForGroup");
        if(type == ChatType.Private)
        {
            dirId = (long)_userStateTelegramRepository.Get((long)id.Identifier).DirectionId;
        }
        else if(type == ChatType.Group)
        {
            dirId = _directionRepository.AuthorizationVerification((long)id.Identifier);
        }
        switch (messageText)
            {
                case "/start":
                case "/Start":
                    {
                        CreateMenuFunctional(id, botClient);
                        return;
                    }
                case "Список дисциплин":
                    {
                        var replyKeyboard = new ReplyKeyboardMarkup(
                            new List<KeyboardButton[]>()
                            {
                                new KeyboardButton[] { ("Сообщение"), "Файл(.pdf)", },
                            });
                        await botClient.SendMessage(
                            id,
                            "Выберите нужный тип отправки:",
                            replyMarkup: replyKeyboard);
                        userState.RequestType = UserStateTypeRequest.DisciplineUpdate;
                        _userStateTelegramRepository.Update(userState);
                        return;
                    }
                case "Список студентов":
                    {
                        var replyKeyboard = new ReplyKeyboardMarkup(
                            new List<KeyboardButton[]>()
                            {
                                new KeyboardButton[] { ("Сообщение"), "Файл(.pdf)", },
                            });
                        await botClient.SendMessage(
                            id,
                            "Выберите нужный тип отправки:",
                            replyMarkup: replyKeyboard);
                        userState.RequestType = UserStateTypeRequest.StudentUpdate;
                        _userStateTelegramRepository.Update(userState);
                        return;
                    }
                case "Расписание":
                    {
                        var replyKeyboard = new ReplyKeyboardMarkup(
                            new List<KeyboardButton[]>()
                            {
                                new KeyboardButton[] { ("Сообщение"), "Файл(.pdf)", },
                            });
                        await botClient.SendMessage(
                            id,
                            "Выберите нужный тип отправки:",
                            replyMarkup: replyKeyboard);
                        userState.RequestType = UserStateTypeRequest.ScheduleUpdate;
                        _userStateTelegramRepository.Update(userState);
                        return;
                    }
                case "Сообщение":
                {
                    UserStateTypeRequest typeRequest = userState.RequestType;
                    if (typeRequest == UserStateTypeRequest.DisciplineUpdate)
                    {
                        await _disSend.DisciplineUpdateAsync(id, botClient, dirId, "text");
                    }
                    else if (typeRequest == UserStateTypeRequest.ScheduleUpdate)
                    {
                        await _scheduleUpdate.ScheduleUpdateAsync(id, botClient, dirId, "text");
                    }
                    else
                    {
                        await _studentUpdate.StudentUpdateAsync(id, botClient, dirId, "text");
                    }    
                    CreateMenuFunctional(id, botClient);
                    return;
                }
                case "Файл(.pdf)":
                {
                    UserStateTypeRequest typeRequest = userState.RequestType;
                    if (typeRequest == UserStateTypeRequest.DisciplineUpdate)
                    {
                        await _disSend.DisciplineUpdateAsync(id, botClient, dirId, "file");
                    }
                    else if (typeRequest == UserStateTypeRequest.ScheduleUpdate)
                    {
                        await _scheduleUpdate.ScheduleUpdateAsync(id, botClient, dirId, "file");
                    }
                    else
                    {
                        await _studentUpdate.StudentUpdateAsync(id, botClient, dirId, "file");
                    }    
                    CreateMenuFunctional(id, botClient);
                    return;
                }
            }
        return;
    }
        
}