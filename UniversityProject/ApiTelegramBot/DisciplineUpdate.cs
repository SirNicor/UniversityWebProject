using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text.Json;
using System.Text;
using Repository;
using SelectPdf;
using Logger;
using IRepositoryAll;
namespace ApiTelegramBot;

public class DisciplineUpdate : IDisciplineUpdate
{
    private IDirectionRepository _dirRepository;
    private ICreateMessageClass _createMessageClass;
    private CreateFileClass _createFileClass;
    private MyLogger _logger;
    public DisciplineUpdate(IDirectionRepository dirRepository, ICreateMessageClass createMessageClass, MyLogger logger, CreateFileClass createFileClass)
    {
        _dirRepository = dirRepository;
        _createMessageClass = createMessageClass;
        _createFileClass = createFileClass;
        _logger = logger;
    }
    public async Task DisciplineUpdateAsync(ChatId id, 
        ITelegramBotClient botClient, long dirId, string type)
    {
        var htmlOfDisciplines = _createMessageClass.DisciplineMessage(_dirRepository.Get(dirId).Disciplines);
        if (type == "text")
        {
            foreach (var discipline in htmlOfDisciplines) 
            {
                await botClient.SendMessage(id, discipline, ParseMode.Html);
            }
        }
        else
        {
            try
            {
                var result = _createFileClass.DisciplineMessage(_dirRepository.Get(dirId).Disciplines);
                await botClient.SendDocument(id, InputFile.FromStream(result, "Дисциплины.pdf"));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, "ApiTelegramBot:DisciplineMethod");
                throw;
            }
        }
    }
}