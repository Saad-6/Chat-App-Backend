using Chat_App.Code;
using Chat_App.Models;
using Chat_App.Models.DTOs;
using Chat_App.Models.Enums;
using Chat_App.Services;
using Chat_App.Services.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace Chat_App.Controllers;
public class AuthController : Controller
{
    private readonly IUserRepository _userRepo;
    private readonly IRepository<Profile> _profile;
    private readonly IRepository<Message> _message;
    private readonly IRepository<FriendRequest> _friendRequest;
    private readonly IRepository<Contact> _contact;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly AuthHandler authHandler;
    private static int count;
    public AuthController(
        IUserRepository userRepo,
        IRepository<Profile> profile,
        UserManager<User>userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        IRepository<Message> message
        ,IRepository<FriendRequest> friendRequest,
        IRepository<Contact> contact
        )
    {
        _userRepo = userRepo;
        _profile = profile;
        _userManager = userManager;
        _signInManager = signInManager;
        authHandler = new AuthHandler(configuration);
        _message = message;
        _friendRequest = friendRequest;
        _contact = contact; 
        count = 0;

    }
    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO formData)
    {
        var respone = new Response()
        {
            Result = formData,
            Success = false
        };
        if (formData == null || (formData.Password != formData.ConfirmPassword))
        {
            respone.Message = "Form Data is not valid";
            return BadRequest(respone);
        }
        else if(await _userRepo.UserExists(formData.Email, formData.Phone))
        {
            respone.Message = "User with these Credentials already exists";
            return BadRequest(respone);
        }
        try
        {
            var user = new User();
            user.Email = formData.Email;
            user.PhoneNumber = formData.Phone;
            user.Name = formData.FullName;
            user.UserName = formData.FullName;
            var result = await _userManager.CreateAsync(user, formData.Password);
            if (!result.Succeeded)
            {
                respone.Message = result.Errors.FirstOrDefault().Description;
                return BadRequest(respone);
            }
            var profile = new Profile()
            {
                UserName = user.Name,
            };
            await _profile.SaveAsync(profile);
            respone.Success = true;
            respone.Message = "Your account has been registered!";
            return Ok(respone);
        }
        catch (Exception ex) 
        {
            respone.Message = ex.Message;
            return BadRequest(respone);
        }
    }
    [HttpPost("SearchContact")]
    public async Task<IActionResult> SearchContact([FromBody] SearchContact query)
    {
        if(string.IsNullOrEmpty(query.UserId) || string.IsNullOrEmpty(query.EmailOrPhone))
        {
            return BadRequest();
        }
        var user = await _userRepo.GetByPhoneOrEmailAsync(query.EmailOrPhone);
        // Make search user invisble if he has already been sent a friend request by the querying user
        var requestsSent = await _friendRequest.GetRequestBySearchParameterAsync("SenderId", query.UserId);
        foreach(var request in requestsSent)
        {
            if(request.RecieverId == user?.Id)
            {
                user = null;
            }
        }
        // Get the required user by search query ( query.EmailOrPhone)
        // Get the current user (query.UserId) 
        // Check the current user's contacts and search if any of those contacts have an email that matches that of the searched user
        // If a match is found, meaning the searched user is already one  of the contacts so , make the result null

        var contactExists = (await _userRepo.GetByIdAsync(query.UserId))?.Contacts?.Any(contact => contact.ContactEmail == user?.Email);
        if (contactExists == true || user.Id == query.UserId)
        {
            user = null;
        }
        var response = new Response()
        {
            Success = true,
            Result = user
        };
        return Ok(response);
    }

    [HttpPost("FriendRequests")]
    public async Task<IActionResult> FriendRequests([FromBody] UserDTO request)
    {
        var userId = request.Id;
        var friendRequests = new List<FriendRequest>();
        var friendRequestResponseModel = new List<FriendRequestDTO>(); 
        
        friendRequests = await _friendRequest.GetRequestBySearchParameterAsync("RecieverId", userId);
        foreach (var friendRequest in friendRequests)
        {
            if(friendRequest.Status == FriendRequestStatus.Pending)
            {
            friendRequestResponseModel.Add(new FriendRequestDTO
            {
                avatar = friendRequest.SenderPicture,
                id = friendRequest.Id,
                name = friendRequest.SenderName
            });

            }
        }
        var response = new Response()
        {
            Result = friendRequestResponseModel,
            Success = true
        };
        return Ok(response);
    }
    [HttpPost("FriendRequests/accept")]
    public async Task<IActionResult> AcceptFriendRequests([FromBody] FriendRequestDTO request)
    {
    var friendRequest = await _friendRequest.LoadByIdAsync(request.id);
        friendRequest.Status = FriendRequestStatus.Accepted;
        await _friendRequest.DeleteAsync(friendRequest);
        var senderUser = await _userRepo.GetByIdAsync(friendRequest.SenderId);
        var recieverUser = await _userRepo.GetByIdAsync(friendRequest.RecieverId);
        var recieverContact = new Contact()
        {
            ContactEmail = senderUser.Email,
            ContactName = senderUser.Name,
            ContactPhone = senderUser.PhoneNumber,
            ContactPicture = senderUser?.Profile?.ProfilePicture
        };
        var senderContact = new Contact()
        {
            ContactEmail = recieverUser.Email,
            ContactName = recieverUser.Name,
            ContactPhone = recieverUser.PhoneNumber,
            ContactPicture = recieverUser?.Profile?.ProfilePicture
        };
        senderUser?.Contacts?.Add(senderContact);
        recieverUser?.Contacts?.Add(recieverContact);
        await _userRepo.UpdateUserAsync(senderUser);
        await _userRepo.UpdateUserAsync(recieverUser);
        var response = new Response()
        {
            Success = true
        };
        return Ok(response);
    }
    [HttpPost("FriendRequests/reject")]
    public async Task<IActionResult> RejectFriendRequests([FromBody] FriendRequestDTO request)
    {
        var friendRequest = await _friendRequest.LoadByIdAsync(request.id);
       await  _friendRequest.DeleteAsync(friendRequest);
        var response = new Response()
        {
            Success = true
        };
        return Ok(response);
    }
    [HttpPost("SendRequest")]
    public async Task<IActionResult> SendRequest([FromBody] AddContact model)
    {
        var response = new Response();
        if (model.SenderUserId == null || model.RecieverUserId == null) return BadRequest();
        var senderUser = await _userRepo.GetByIdAsync(model.SenderUserId);
        var recieverUser = await _userRepo.GetByIdAsync(model.RecieverUserId);
        if(senderUser.Contacts.Any(m=>m.ContactEmail == recieverUser.Email))
        {
            return BadRequest(response);
        }
        if(senderUser == null || recieverUser == null || (senderUser.Id == recieverUser.Id))
        {
            response.Success = false;
            response.Message = "User(s) not found";
            return BadRequest(response);
        } 
        var friendRequest = new FriendRequest()
        {
            SenderId = senderUser.Id,
            RecieverId = recieverUser.Id,
            SenderName = senderUser.Name,
            SenderPicture = "Will change this later"
        }; 
        try
        {
            await _friendRequest.SaveAsync(friendRequest);
        }
        catch (DbUpdateException ex) 
        {

            if (ex.InnerException != null)
            {
                Console.Error.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
        // For some reason uncommenting these lines causes the endppoint to run twice
        //response.Success = true;
        //response.Message = "Request Sent!";
        return Ok(response);
    }
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO formData)
    {
        var respone = new Response()
        {
            Success = false
        };
        var user = await _userRepo.GetByEmailAsync(formData.Email);
        if (formData == null)
        {
            respone.Message = "Form Data is not valid";
            return BadRequest(respone);
        }
        else if (user == null)
        {
            respone.Message = "User does not exist";
            return BadRequest(respone);
        }
        bool isValid = await _userManager.CheckPasswordAsync(user, formData.Password);
        if (!isValid)
        {
            respone.Success = false;
            respone.Message = "Wrong Credentials";
            return BadRequest(respone);
        }
        try
        {
        respone.Result = new LoginResponseDTO()
        {
            JWT = authHandler.GetJWT(user),
            Email = formData.Email,
        };
        }
        catch(Exception ex)
        {
            respone.Message = ex.Message;
            return BadRequest(respone);
        }
        respone.Success = true;
        return Ok(respone);
    }
}
