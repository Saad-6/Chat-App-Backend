using Microsoft.AspNetCore.Identity;

namespace Chat_App.Models;

public class User : IdentityUser 
{
    public bool IsOnline {  get; set; } = true;
    public DateTime LastSeen { get; set; } = DateTime.Now;
    public string? Name { get; set; }
    public IList<Contact>? Contacts { get; set; } = new List<Contact>();
    public IList<Chat>? Chats { get; set; } = new List<Chat>();
    public Profile? Profile { get; set; } = new Profile();
    
}
