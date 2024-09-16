namespace Chat_App.Models.DTOs
{
    public class SearchContact
    {
       public string? EmailOrPhone { get; set; }
       public string? UserId { get; set; }
    }
    public class AddContact
    {
        public string? SenderUserId { get; set; }
        public string? RecieverUserId { get; set; }
    }
 
    public class UserDTO
    {
      public  string? Id { get; set; }
    }
}
