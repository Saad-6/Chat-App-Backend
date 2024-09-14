namespace Chat_App.Models;

public class ContactList : BaseEntity
{
    public User? Owner { get; set; }
    public IList<User>? Contacts { get; set; } = new List<User>();
 
}
