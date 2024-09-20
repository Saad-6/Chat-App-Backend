using Chat_App.Code;
using Chat_App.Models;
using Chat_App.Models.DTOs;
using Chat_App.Services;
using Chat_App.Services.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Chat_App.Controllers;

public class ChatController : Controller
{
    private readonly IRepository<Profile> _profile;
    private readonly IRepository<Message> _message;
    private readonly UserManager<User> _userManager;
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepo;
    private readonly WebSocketHandler _webSocketHandler;
    public ChatController(IRepository<Profile> profile, IRepository<Message> message, UserManager<User> userManager, IChatRepository chatRepository, IUserRepository userRepo)
    {
        _profile = profile;
        _message = message;
        _userManager = userManager;
        _chatRepository = chatRepository;
        _userRepo = userRepo;
        _webSocketHandler = new WebSocketHandler();
    }
    [HttpPost("GetChatThumbnails")]
    public async Task<IActionResult> GetChatThumbnails([FromBody] UserDTO model)
    {
        var chatThumbNails = new List<UserChatsDTO>();

        // Fetch all contacts at once
        var contacts = await _userRepo.GetContactsAsync(model.Id);
        if (contacts == null || !contacts.Any()) return Ok();

        // Get all contact emails in one call to avoid repeated calls
        var contactEmails = contacts.Select(c => c.ContactEmail).ToList();
        var contactUserIds = await _userRepo.GetUserIdsByEmailAsync(contactEmails); 

        // Fetch all chats for the user and filter by participant IDs
        var userChats = await _chatRepository.GetUserChatsAsync(model.Id);

        var chatIds = userChats.Select(c => c.Id).ToList();
        var lastMessages = await _chatRepository.GetLastMessagesInBulkAsync(chatIds); // New bulk method for last messages

        // Map contacts to chat thumbnails
        foreach (var contact in contacts)
        {
            if (!contactUserIds.TryGetValue(contact.ContactEmail, out var contactUserId)) continue; // Skip if no user found

            var chat = userChats.FirstOrDefault(c => c.Participants.Any(p => p.Id == contactUserId));
            var chatId = chat?.Id ?? 0;

            var lastMessage = lastMessages
            .FirstOrDefault(m => m.Key == chatId).Value ?? new Message { Content = "Start Chatting Now!" };


            chatThumbNails.Add(new UserChatsDTO
            {
                Id = contact.Id,
                ChatId = chatId,
                LastMessage = new MessageDTO
                {
                    Id = lastMessage.Id,
                    Content = lastMessage.Content,
                    SenderUserId = lastMessage.SenderUser?.Id,
                    SentTime = lastMessage.SentTime
                },
                ContactEmail = contact.ContactEmail,
                ContactId = contactUserId,
                ContactName = contact.ContactName,
                ContactPhone = contact.ContactPhone,
                ContactPicture = contact.ContactPicture
            });
        }
        return Ok(new Response
        {
            Result = chatThumbNails,
            Success = true
        });
    }

    [HttpPost("GetOnlineStatus")]
    public async Task<IActionResult> GetOnlineStatus([FromBody] UserDTO model)
    {
        return Ok(new Response { Result = await _webSocketHandler.UserIsOnline(model.Id),Success = true});
    }
    [HttpPost("UnFriend")]
    public async Task<IActionResult> UnFriend([FromBody] ParticipantsDTO model)
    {
        var userIds = new List<string> { model.SenderUserId, model.RecieverUserId };
        var users = await _userRepo.GetUsersInBulkAsync(userIds);
        if (users.Count != 2)
        {
            return NotFound("One or both users not found.");
        }

        var senderUser = users.FirstOrDefault(u => u.Id == model.SenderUserId);
        // var receiverUser = users.FirstOrDefault(u => u.Id == model.RecieverUserId);
        var response = new Response();
        response.Success = true;

        return Ok(response);
    }
    [HttpPost("GetChatMessages")]
    public async Task<IActionResult> GetChatMessages([FromBody] ChatRequestModel request)
    {

        if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.OtherUserId))
        {
            return BadRequest(new Response
            {
                Success = false,
                Message = "Invalid request parameters"
            });
        }
        var users = await _userRepo.GetUsersInBulkAsync([request.UserId,request.OtherUserId]);
        var currentUser = users.Where(m=>m.Id == request.UserId).FirstOrDefault();
        var otherUser = users.Where(m => m.Id == request.OtherUserId).FirstOrDefault();

        if (currentUser == null || otherUser == null)
        {
            return NotFound(new Response
            {
                Success = false,
                Message = "One or both users not found"
            });
        }
        var chat = await _chatRepository.GetChat([request.UserId, request.OtherUserId]);
        if (chat == null)
        {
            chat = new Chat
            {
                Participants = new List<User> { currentUser, otherUser },
                Messages = new List<Message>()
            };
            await _chatRepository.SaveAsync(chat);
        }
        var response = new Response
        {
            Result = new ChatResponseModel
            {
                ChatId = chat.Id,
                Participants = chat.Participants.Select(p => new UserModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    IsOnline = p.IsOnline,
                    LastSeen = p.LastSeen
                }).ToList(),
                Messages = chat.Messages.Select(m => new MessageModel
                {
                    Id = m.Id,
                    Content = m.Content,
                    SenderUserId = m.SenderUser.Id,
                    ReceiverUserId = m.ReceiverUser.Id,
                    SentTime = m.SentTime,
                    ReadTime = m.ReadTime,
                    ReadStatus = m.ReadStatus
                }).OrderBy(m => m.SentTime).ToList()
            },
            Success = true
        };
        return Ok(response);
    }

    [HttpPost("SendMessage")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        if (string.IsNullOrEmpty(request.SenderId) || string.IsNullOrEmpty(request.ReceiverId) || string.IsNullOrEmpty(request.Content))
        {
            return BadRequest(new Response
            {
                Success = false,
                Message = "Invalid request parameters"
            });
        }

        var users = await _userRepo.GetUsersInBulkAsync([request.SenderId, request.ReceiverId]);
        var sender = users.Where(m => m.Id == request.SenderId).FirstOrDefault();
        var receiver = users.Where(m => m.Id == request.ReceiverId).FirstOrDefault();

        bool isNewChat = false;
        if (sender == null || receiver == null)
        {
            return NotFound(new Response
            {
                Success = false,
                Message = "Sender or receiver not found"
            });
        }
        var chat = await _chatRepository.GetChat([request.SenderId, request.ReceiverId]);
        if(chat?.Messages?.Count == 0)
        {
            isNewChat = true;
        }
        if (chat == null)
        {
            chat = new Chat
            {
                Participants = new List<User> { sender, receiver },
                Messages = new List<Message>()
            };
            await _chatRepository.SaveAsync(chat);
        }
        var message = new Message
        {
            Content = request.Content,
            SenderUser = sender,
            ReceiverUser = receiver,
            ChatId = chat.Id,
            Chat = chat,
            SentTime = DateTime.UtcNow,
            ReadStatus = false
        };

        chat?.Messages?.Add(message);
        await _chatRepository.UpdateAsync(chat);

        var response = new Response
        {
            Result = new SendMessageResponse
            {
                MessageId = message.Id,
                ChatId = chat.Id,
                SentTime = message.SentTime
            },
            Success = true,
            Message = "Message sent successfully"
        };

        if (isNewChat)
        {
            //var chatThumbNails = new List<UserChatsDTO>();
            //var contacts = await _userRepo.GetContactsAsync(request.ReceiverId);
            //foreach (var contact in contacts)
            //{
            //    var contactUserId = await _userRepo.GetUserIdByPhoneOrEmailAsync(contact.ContactEmail);
            //    if (contactUserId == null)
            //    {
            //        continue;
            //    }
            //    var individualChat = await _chatRepository.GetChat(new[] { request.ReceiverId, contactUserId }.ToList());
            //    var chatId = individualChat == null ? 0 : individualChat.Id;

            //    var lastMessage = await _chatRepository.GetLastMessageAsync(chatId);
            //    chatThumbNails.Add(new UserChatsDTO
            //    {
            //        Id = contact.Id,
            //        ChatId = chatId,
            //        LastMessage = lastMessage != null ? new MessageDTO
            //        {
            //            Id = lastMessage.Id,
            //            Content = lastMessage.Content,
            //            SenderUserId = lastMessage.SenderUser?.Id,
            //            SentTime = lastMessage.SentTime
            //        } : new MessageDTO { Content = "Start Chatting Now!" },
            //        ContactEmail = contact.ContactEmail,
            //        ContactId = contactUserId,
            //        ContactName = contact.ContactName,
            //        ContactPhone = contact.ContactPhone,
            //        ContactPicture = contact.ContactPicture,
            //    });

            //}

            var endResult = new
            {
                type = "thumbnaillist",
                //data = chatThumbNails
            };
            await _webSocketHandler.SendMessageToUserAsync(request.ReceiverId, JsonConvert.SerializeObject(endResult));

        }
        else
        {
            var messagePayload = new
            {
                id = message.Id,
                content = message.Content,
                senderUserId = request.SenderId,
                receiverUserId = request.ReceiverId,
                sentTime = message.SentTime
            };
            var endResult = new
            {
                type = "message",
                data = messagePayload
            };
            await _webSocketHandler.SendMessageToUserAsync(request.ReceiverId, JsonConvert.SerializeObject(endResult));

        }
        return Ok(response);
    }
    [HttpPost("GetUserInfo")]
    public async Task<IActionResult> GetUserInfo([FromBody] UserDTO model)
    {
        var user = await _userRepo.GetByIdAsync(model.Id);
        if (user == null)
        {
            return NotFound(new Response
            {
                Success = false,
                Message = "User not found"
            });
        }

        var response = new Response
        {
            Result = new UserModel
            {
                Id = user.Id,
                Name = user.Name,
                IsOnline = user.IsOnline,
                LastSeen = user.LastSeen
            },
            Success = true
        };
        return Ok(response);
    }
}
