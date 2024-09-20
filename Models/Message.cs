using System.ComponentModel.DataAnnotations.Schema;

namespace Chat_App.Models;

public class Message : BaseEntity
{
    public string? Content { get; set; }
    public User? SenderUser { get; set; }
    public User? ReceiverUser { get; set; }
    public int  ChatId { get; set; }
    public Chat? Chat { get; set; }
    public DateTime SentTime { get; set; } = DateTime.Now;
    public DateTime ReadTime { get; set; }
    public bool ReadStatus { get; set; } = false;
}
