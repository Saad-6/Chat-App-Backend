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
    private readonly WebSocketHandler _webSocketHandler ;
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
        var contacts = await _userRepo.GetContactsAsync(model.Id);
        if (contacts == null) return Ok();
        var defaultMessage = new Message()
        {
            Content = "Start Chatting Now!"
        };
        foreach (var contact in contacts)
        {
            var contactUserId = await _userRepo.GetUserIdByPhoneOrEmailAsync(contact.ContactEmail);
            if (contactUserId == null)
            {
                continue;
            }
            var individualChat = await _chatRepository.GetChat(new[] { model.Id, contactUserId }.ToList());
            var chatId = individualChat == null ? 0 : individualChat.Id;

            var lastMessage = await _chatRepository.GetLastMessageAsync(chatId);
            chatThumbNails.Add(new UserChatsDTO
            {
                Id = contact.Id,
                ChatId = chatId,
                LastMessage = lastMessage != null ? new MessageDTO
                {
                    Id = lastMessage.Id,
                    Content = lastMessage.Content,
                    SenderUserId = lastMessage.SenderUser?.Id,
                    SentTime = lastMessage.SentTime
                } : new MessageDTO { Content = "Start Chatting Now!" },
                ContactEmail = contact.ContactEmail,
                ContactId = contactUserId,
                ContactName = contact.ContactName,
                ContactPhone = contact.ContactPhone,
                ContactPicture = contact.ContactPicture,
            });
        }
        var response = new Response()
        {
            Result = chatThumbNails,
            Success = true
        };
        return Ok(response);
    }

    [HttpPost("GetChatMessages")]
    public async Task<IActionResult> GetChatMessages([FromBody] ChatRequestModel request)
    {
        if (request == null || string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.OtherUserId))
        {
            return BadRequest(new Response
            {
                Success = false,
                Message = "Invalid request parameters"
            });
        }

        var currentUser = await _userRepo.GetByIdAsync(request.UserId);
        var otherUser = await _userRepo.GetByIdAsync(request.OtherUserId);

        if (currentUser == null || otherUser == null)
        {
            return NotFound(new Response
            {
                Success = false,
                Message = "One or both users not found"
            });
        }

        var participants = new List<string> { request.UserId, request.OtherUserId };
        var chat = await _chatRepository.GetChat(participants);

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
        if (request == null || string.IsNullOrEmpty(request.SenderId) || string.IsNullOrEmpty(request.ReceiverId) || string.IsNullOrEmpty(request.Content))
        {
            return BadRequest(new Response
            {
                Success = false,
                Message = "Invalid request parameters"
            });
        }

        var sender = await _userRepo.GetByIdAsync(request.SenderId);
        var receiver = await _userRepo.GetByIdAsync(request.ReceiverId);

        if (sender == null || receiver == null)
        {
            return NotFound(new Response
            {
                Success = false,
                Message = "Sender or receiver not found"
            });
        }

        var participants = new List<string> { request.SenderId, request.ReceiverId };
        var chat = await _chatRepository.GetChat(participants);

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

        chat.Messages.Add(message);
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
  //      export interface MessageModel
  //  {
  //      id: number;
  //  content: string;
  //  senderUserId: string;
  //  receiverUserId: string;
  //  sentTime: string;
  //  readTime: string;
  //  readStatus: boolean;
  //}
    var messagePayload = new
        {
            id = message.Id,
            content = message.Content,
            senderUserId = request.SenderId,
            receiverUserId = request.ReceiverId,
            sentTime = message.SentTime
        };
        await _webSocketHandler.SendMessageToUserAsync(request.ReceiverId, JsonConvert.SerializeObject(messagePayload));

        return Ok(response);
    }

}
