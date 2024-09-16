using Chat_App.Models.Enums;

namespace Chat_App.Models
{
    public class FriendRequest : BaseEntity
    {
        public string SenderId {get; set;}
        public string RecieverId {get; set;}
        public string SenderName {get; set;}
        public string SenderPicture {get; set;}
        public FriendRequestStatus Status {get; set;} = FriendRequestStatus.Pending;
    }
}
