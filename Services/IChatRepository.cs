using Chat_App.Models;
using Chat_App.Services.Base;

namespace Chat_App.Services;

public interface IChatRepository : IRepository<Chat>
{
    Task<Message> GetMessageByIdAsync(int id);
    Task<Message> GetLastMessageAsync(int chatId);
    Task<bool> DeleteChatAsync(Chat chat);
    Task<bool> DeleteMessageAsync(Message message);
    Task<Chat> GetChat(List<string> participants);
    Task<IList<Chat>> GetUserChatsAsync(string userId);
}
