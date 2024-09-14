using Chat_App.Code;
using Chat_App.Models;
using Chat_App.Models.DTOs;
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
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly AuthHandler authHandler;
    public AuthController(IUserRepository userRepo, IRepository<Profile> profile, UserManager<User>userManager,SignInManager<User> signInManager, IConfiguration configuration,IRepository<Message> message)
    {
        _userRepo = userRepo;
        _profile = profile;
        _userManager = userManager;
        _signInManager = signInManager;
        authHandler = new AuthHandler(configuration);
        _message = message;

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
        var user = await _userRepo.GetByPhoneOrEmailAsync(query.EmailOrPhone);
        var response = new Response()
        {
            Success = true,
            Result = user
        };
        return Ok(response);
    }
    [HttpPost("GetUser")]
    public async Task<IActionResult> GetUser([FromBody] UserDTO model)
    {
        if(model == null) {
            return BadRequest();
        }
        var user = await _userRepo.GetByIdAsync(model.Id);
        var messages = await _message.LoadAllAsync();
        var lastMessage = messages.LastOrDefault();
         var response = new Response()
        {
            Result = user,
            Success = true
        };
        return Ok(response);
    }

    [HttpPost("AddContact")]
    public async Task<IActionResult> AddContact([FromBody] AddContact model)
    {
        if (model.SenderUserId == null || model.RecieverUserId == null) return BadRequest();
        var senderUser = await _userRepo.GetByIdAsync(model.SenderUserId);
        var recieverUser = await _userRepo.GetByIdAsync(model.RecieverUserId);
        var response = new Response();
        if(senderUser == null || recieverUser == null || (senderUser.Id == recieverUser.Id))
        {
            response.Success = false;
            response.Message = "User(s) not found";
            return BadRequest(response);
        }
        bool senderHasContact = senderUser.Contacts.Any(c => c.ContactEmail == recieverUser.Email);
        if (senderHasContact)
        {
            response.Success = false;
            response.Message = "Contact already exists";
            return BadRequest(response);
        }
        var recieverContact = new Contact()
        {
            ContactEmail = senderUser.Email,
            ContactName = senderUser.Name,
            ContactPhone = senderUser.PhoneNumber,
            ContactPicture = senderUser.Profile.ProfilePicture
        };
        var senderContact = new Contact()
        {
            ContactEmail = recieverUser.Email,
            ContactName = recieverUser.Name,
            ContactPhone = recieverUser.PhoneNumber,
            ContactPicture = recieverUser.Profile.ProfilePicture
        };
     
        senderUser.Contacts.Add(senderContact);
        recieverUser.Contacts.Add(recieverContact);
        await _userRepo.UpdateUserAsync(senderUser);
        await _userRepo.UpdateUserAsync(recieverUser);
        response.Success = true;
        response.Message = "Contact Added !";
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
