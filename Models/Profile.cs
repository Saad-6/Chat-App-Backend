namespace Chat_App.Models;

public class Profile : BaseEntity
{
    public string? ProfilePicture {  get; set; }
    public DateTime JoinedDate { get; set; } 
    public string? UserName { get; set; }
    public string? Description { get; set; }
    public Profile()
    {
        JoinedDate = DateTime.Now;
        ProfilePicture = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSb51ZwKCKqU4ZrB9cfaUNclbeRiC-V-KZsfQ&s";
        Description = "Just Joined !";
    }
}
