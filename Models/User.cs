using Microsoft.AspNetCore.Identity;

namespace Chat_App.Models;

public class User : IdentityUser 
{
    public bool IsOnline {  get; set; } = true;
    public DateTime LastSeen { get; set; } = DateTime.Now;
    public string? Name { get; set; }
    public ContactList? Contacts { get; set; } = new ContactList();
    public IList<Chat>? Chats { get; set; } = new List<Chat>();
    public Profile? Profile { get; set; } = new Profile();
}
