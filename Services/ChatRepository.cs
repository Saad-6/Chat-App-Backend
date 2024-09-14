using Chat_App.Code;
using Chat_App.Data;
using Chat_App.Models;
using Chat_App.Services.Base;
using Microsoft.EntityFrameworkCore;

namespace Chat_App.Services;

public class ChatRepository : Repository<Chat>, IChatRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<Chat> _dbSetChat;  
    private readonly DbSet<Message> _dbSetMessage;
    public ChatRepository(AppDbContext context) : base(context)
    {
        _context = context;
        _dbSetChat = _context.Set<Chat>();
        _dbSetMessage = _context.Set<Message>();
    }

    public async Task<bool> DeleteChatAsync(Chat chat)
    {
        if(Utility.Null(chat)) return false;
        _dbSetChat.Remove(chat);
         return true;
    }

    public async Task<bool> DeleteMessageAsync(Message message)
    {
        if (Utility.Null(message)) return false;
        _dbSetMessage.Remove(message);
        return true;
    }

    public async Task<Chat> GetChat(List<string> participants)
    {
        if (Utility.Null(participants)) return null;
        var query = _dbSetChat
            .Include(c => c.Participants) 
            .Include(c=>c.Messages)
            .Where(c => c.Participants.All(p => participants.Contains(p.Id)));
        return await query.FirstOrDefaultAsync();
    }

    public async Task<Message> GetLastMessageAsync(int chatId)
    {
        if (Utility.Null(chatId)) return null;
        return await _dbSetMessage
     .Where(m => m.ChatId == chatId)
     .OrderByDescending(m => m.SentTime)
     .FirstOrDefaultAsync();
    }

    public async Task<Message> GetMessageByIdAsync(int messageId)
    {
        if (Utility.Null(messageId)) return null;
        return await _dbSetMessage.FindAsync(messageId);
    }

    public async Task<IList<Chat>> GetUserChatsAsync(string userId)
    {
        if (Utility.Null(userId)) return null;
        return  await _dbSetChat
           .Include(c => c.Participants) 
           .Where(c => c.Participants.Any(p => p.Id == userId))
           .ToListAsync();
    }
}
