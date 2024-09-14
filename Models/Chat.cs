namespace Chat_App.Models;

public class Chat : BaseEntity
{
    public IList<User>? Participants { get; set; }
    public IList<Message>? Messages { get; set; }
    public Chat() 
    {
        Participants = new List<User>();
        Messages = new List<Message>();
    }
}
