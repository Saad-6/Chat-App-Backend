namespace Chat_App.Models;

public class ViewModels
{

}
public class UserChats : Contact
{
    public int ChatId { get; set; }
    public string ContactId { get; set; }
    public Message LastMessage { get; set; }
}

public class ChatRequestModel
{
    public string UserId { get; set; }
    public string OtherUserId { get; set; }
}
public class MessageDTO
{
    public int Id { get; set; }
    public string Content { get; set; }
    public string SenderUserId { get; set; }
    public DateTime SentTime { get; set; }
}

public class UserChatsDTO
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public MessageDTO LastMessage { get; set; }
    public string ContactEmail { get; set; }
    public string ContactId { get; set; }
    public string ContactName { get; set; }
    public string ContactPhone { get; set; }
    public string ContactPicture { get; set; }
}

public class ChatResponseModel
{
    public int ChatId { get; set; }
    public List<UserModel> Participants { get; set; }
    public List<MessageModel> Messages { get; set; }
}

public class UserModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; }
}

public class MessageModel
{
    public int Id { get; set; }
    public string Content { get; set; }
    public string SenderUserId { get; set; }
    public string ReceiverUserId { get; set; }
    public DateTime SentTime { get; set; }
    public DateTime ReadTime { get; set; }
    public bool ReadStatus { get; set; }
}