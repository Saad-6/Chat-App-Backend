using Microsoft.AspNetCore.Identity;

namespace Chat_App.Models;

public class User : IdentityUser 
{
    public bool IsOnline {  get; set; }
    public DateTime LastSeen { get; set; }
    public string? Name { get; set; }
    public ContactList? Contacts { get; set; }
    public IList<Chat> Chats { get; set; } = new List<Chat>();
    public Profile? Profile { get; set; }
}
