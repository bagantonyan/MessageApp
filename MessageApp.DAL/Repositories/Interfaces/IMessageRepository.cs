using MessageApp.DAL.Entities;

namespace MessageApp.DAL.Repositories.Interfaces
{
    public interface IMessageRepository
    {
        Task<int> AddMessageAsync(Message message);
        Task<List<Message>> GetMessagesAsync(DateTime startTime);
        Task<int> GetNextSequenceNumberAsync();
    }
}