namespace Chat_App.Models
{
    public class MessageViewModels
    {
    }

    public class SendMessageRequest
    {
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Content { get; set; }
    }

    public class SendMessageResponse
    {
        public int MessageId { get; set; }
        public int ChatId { get; set; }
        public DateTime SentTime { get; set; }
    }

}
