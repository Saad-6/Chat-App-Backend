using Chat_App.Code;
using Chat_App.Models;
using Chat_App.Models.DTOs;
using Chat_App.Models.Enums;
using Chat_App.Services;
using Chat_App.Services.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            var user = new User()
            {
                Email = formData.Email,
                PhoneNumber = formData.Phone,
                Name = formData.FullName,
                UserName = formData.FullName,
            };
           
            var result = await _userManager.CreateAsync(user, formData.Password);
            if (!result.Succeeded)
            {
                respone.Message = result.Errors?.FirstOrDefault()?.Description;
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
        var response = new Response()
        {
            Success = true,
            Result = user
        };
        // Make search user invisble if he has already been sent a friend request by the querying user
        var requestsSent = await _friendRequest.GetRequestBySearchParameterAsync("SenderId", query.UserId,true) as List<FriendRequest>;
        if (requestsSent?.Count > 0)
        {
            foreach (var request in requestsSent)
            {
                if (request.RecieverId == user?.Id)
                {
                    response.Result = null;
                    return Ok(response);
                }
            }
        }
        // Get the required user by search query ( query.EmailOrPhone)
        // Get the current user (query.UserId) 
        // Check the current user's contacts and search if any of those contacts have an email that matches that of the searched user
        // If a match is found, meaning the searched user is already one  of the contacts so , make the result null
        var contactExists = (await _userRepo.GetByIdAsync(query.UserId))?.Contacts?.Any(contact => contact.ContactEmail == user?.Email);
        if (contactExists == true || user?.Id == query.UserId)
        {
            response.Result = null;
        }
    
        return Ok(response);
    }

    [HttpPost("FriendRequests")]
    public async Task<IActionResult> FriendRequests([FromBody] UserDTO request)
    {
        var userId = request.Id;
        var friendRequests = new List<FriendRequest>();
        var friendRequestResponseModel = new List<FriendRequestDTO>(); 
        
        friendRequests = await _friendRequest.GetRequestBySearchParameterAsync("RecieverId", userId,true) as List<FriendRequest>;
        if (friendRequests?.Count > 0)
        {
            foreach (var friendRequest in friendRequests)
            {
                if (friendRequest.Status == FriendRequestStatus.Pending)
                {
                    friendRequestResponseModel.Add(new FriendRequestDTO
                    {
                        avatar = friendRequest.SenderPicture,
                        id = friendRequest.Id,
                        name = friendRequest.SenderName
                    });
                }
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
        if (friendRequest == null) return NotFound("Friend request not found.");

        friendRequest.Status = FriendRequestStatus.Accepted;
        await _friendRequest.DeleteAsync(friendRequest);

        var senderUser = await _userRepo.GetByIdAsync(friendRequest.SenderId);
        var receiverUser = await _userRepo.GetByIdAsync(friendRequest.RecieverId);

        if (senderUser == null || receiverUser == null) return NotFound("Users not found.");

        AddContact(senderUser, receiverUser);
        AddContact(receiverUser, senderUser);

        await _userRepo.UpdateUserAsync(senderUser);
        await _userRepo.UpdateUserAsync(receiverUser);

        return Ok(new { Success = true });
    }
    private void AddContact(User user, User contactUser)
    {
        var contact = new Contact
        {
            ContactEmail = contactUser.Email,
            ContactName = contactUser.Name,
            ContactPhone = contactUser.PhoneNumber,
            ContactPicture = contactUser?.Profile?.ProfilePicture
        };
        user?.Contacts?.Add(contact);
    }

    [HttpPost("FriendRequests/reject")]
    public async Task<IActionResult> RejectFriendRequests([FromBody] FriendRequestDTO request)
    {
       var friendRequest = await _friendRequest.LoadByIdAsync(request.id);
       await  _friendRequest.DeleteAsync(friendRequest);

        return Ok(new Response { Success = true});
    }
    [HttpPost("SendRequest")]
    public async Task<IActionResult> SendRequest([FromBody] ParticipantsDTO model)
    {
        var response = new Response()
        {
            Success = false,
            Message = "One or more users not found"
        };
        if (model.SenderUserId == null || model.RecieverUserId == null) return BadRequest(response);

        var users = await _userRepo.GetUsersInBulkAsync([model.SenderUserId, model.RecieverUserId]);
        var senderUser =  users.Where(m=>m.Id == model.SenderUserId).FirstOrDefault();
        var recieverUser = users.Where(m => m.Id == model.RecieverUserId).FirstOrDefault();

        if((senderUser == null || recieverUser == null) || (senderUser.Contacts.Any(m => m.ContactEmail == recieverUser.Email))) return BadRequest(response);

        var friendRequest = new FriendRequest()
        {
            SenderId = senderUser.Id,
            RecieverId = recieverUser.Id,
            SenderName = senderUser.Name,
            SenderPicture = "Will change this later"
        }; 
            await _friendRequest.SaveAsync(friendRequest);
           
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
        if (formData.Email == null || formData.Password == null)
        {
            respone.Message = "Form Data is not valid";
            return BadRequest(respone);
        }
        var user = await _userRepo.GetByEmailAsync(formData.Email);
        if (user == null)
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
