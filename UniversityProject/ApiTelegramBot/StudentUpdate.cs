using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text.Json;
using System.Text;
using Repository;
using SelectPdf;
using Logger;
using Microsoft.Extensions.Logging;
using IRepositoryAll;

namespace ApiTelegramBot;

public class StudentUpdate : IStudentUpdate
{
    private IDirectionRepository _directionRepository;
    private ICreateMessageClass _createMessageClass;
    private MyLogger _logger;
    private CreateFileClass _createFileClass;
    public StudentUpdate(IDirectionRepository directionRepository, ICreateMessageClass createMessageClass, 
        MyLogger logger, CreateFileClass createFileClass)
    {
        _directionRepository = directionRepository;
        _createMessageClass = createMessageClass;
        _createFileClass = createFileClass;
        _logger = logger;
    }
    public async Task StudentUpdateAsync(ChatId id, ITelegramBotClient botClient, long dirId, string type)
    {
        var htmlOfstudents = _createMessageClass.StudentMessage(_directionRepository.Get(dirId).Students);
        if (type == "text")
        {
            foreach (var student in htmlOfstudents)
            {
                await botClient.SendMessage(id, student, ParseMode.Html);
            }
        }
        else
        {
            try
            {
                var result = _createFileClass.StudentMessage(_directionRepository.Get(dirId).Students);
                await botClient.SendDocument(id, InputFile.FromStream(result, "Студенты.pdf"));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, "ApiTelegramBot:StudentUpdate");
                throw;
            }
        }
    }
}