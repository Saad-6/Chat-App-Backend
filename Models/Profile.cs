namespace Chat_App.Models;

public class Profile : BaseEntity
{
    public string? ProfilePicture {  get; set; }
    public DateTime JoinedDate { get; set; }
    public string? UserName { get; set; }
    public string? Description { get; set; }
    public User? User { get; set; }
}
