using Chat_App.Code;
using Chat_App.Models;
using Chat_App.Services.Base;
using Chat_App.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Chat_App.Models.DTOs;

namespace Chat_App.Controllers;

public class CallController : Controller
{
    private readonly IRepository<Profile> _profile;
    private readonly IRepository<Message> _message;
    private readonly UserManager<User> _userManager;
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepo;
    private readonly WebSocketHandler _webSocketHandler;
    public CallController(IRepository<Profile> profile, IRepository<Message> message, UserManager<User> userManager, IChatRepository chatRepository, IUserRepository userRepo)
    {
        _profile = profile;
        _message = message;
        _userManager = userManager;
        _chatRepository = chatRepository;
        _userRepo = userRepo;
        _webSocketHandler = new WebSocketHandler();
    }

    [HttpPost("Ring")]
    public async Task<IActionResult> Ring([FromBody] ParticipantsDTO model)
    {
        return Ok();
    }

}
