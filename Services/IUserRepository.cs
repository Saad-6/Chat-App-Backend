using Chat_App.Models;
using Chat_App.Services.Base;

namespace Chat_App.Services;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetByEmailAsync(string email);
    Task<bool> UserExists(string email = "", string phone = "");
    Task<User> GetByIdAsync(string id);
    Task<User> GetByPhoneAsync(string phone);
    Task<bool> AddUserAsync(User user);
    Task<bool> UpdateUserAsync(User user);

}
