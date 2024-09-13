namespace Chat_App.Models
{
    public class Contact : BaseEntity
    {
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactPicture { get; set; }
    }
}
