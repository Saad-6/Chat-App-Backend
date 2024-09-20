using Chat_App.Code;
using Chat_App.Data;
using Chat_App.Models;
using Chat_App.Services.Base;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Chat_App.Services;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<User> _dbSet;
    public UserRepository(AppDbContext context) : base(context)
    {
        _context = context;
        _dbSet = _context.Set<User>();
    }
    public async Task<bool> AddUserAsync(User user)
    {
        if(Utility.Null(user)) return false;
        var result = await SaveAsync(user); 
        return result;
    }
    public async Task<List<User>> GetUsersInBulkAsync(List<string> userIds)
    {
        if (userIds == null || userIds.Count == 0) return new List<User>();

        return await _dbSet.Where(u => userIds.Contains(u.Id))
            .Include(m=>m.Contacts)
            .Include(m=>m.Profile)
            .ToListAsync();
    }
    public async Task<bool> UserExists(string email = "", string phone = "")
    {
        return await _dbSet.AnyAsync(u => u.Email == email || u.PhoneNumber == phone);
    }
    public async Task<bool> UpdateUserAsync(User user)
    {
        if(Utility.Null(user)) return false;
        var result = await UpdateAsync(user);
        return result;
    }
    public async Task<User> GetByPhoneOrEmailAsync(string query)
    {
        if (Utility.Null(query)) return null;
        if (query.Contains("@"))
        {
            return await GetByEmailAsync(query);
        }
        return await GetByPhoneAsync(query);
    }
    public async Task<User> GetByEmailAsync(string email)
    {
        if(Utility.Null(email)) return null;
        var alsoUser = await GetRequestBySearchParameterAsync("Email", email,false,"Contacts","Profile");
        var user = alsoUser as User;

        return user;
    }
    public async Task<User> GetByIdAsync(string id)
    {
        if (Utility.Null(id)) return null;
        var alsoUser = await GetRequestBySearchParameterAsync("Id", id, false, "Contacts", "Profile");
        var user = alsoUser as User;
        return user != null ? user : new User();
    }
    public async Task<User> GetByPhoneAsync(string phone)
    {
        if (Utility.Null(phone)) return null;
        var alsoUser = await GetRequestBySearchParameterAsync("PhoneNumber", phone, false, "Contacts", "Profile");
        var user = alsoUser as User;
        return user != null ? user : new User();
    }
    public async Task<IList<Contact>> GetContactsAsync(string userId)
    {
        if(Utility.Null(userId)) return null;
        var user = await GetByIdAsync(userId);
        if(Utility.Null(user)) return null;
        return user.Contacts;
    }
    public async Task<string> GetUserIdByPhoneOrEmailAsync(string query)
    {
        var user = await GetByPhoneOrEmailAsync(query);
        return user.Id;
    }
    public async Task<Dictionary<string, string>> GetUserIdsByEmailAsync(List<string> queries)
    {
        if (queries == null || !queries.Any()) return new Dictionary<string, string>();

        var users = await _dbSet
            .Where(u => queries.Contains(u.Email) || queries.Contains(u.PhoneNumber)) 
            .ToListAsync();

        return users.ToDictionary(u => u.Email ?? u.PhoneNumber, u => u.Id);
    }

    public async Task<string> GetUserNameByIdAsync(string userId)
    {
        var userName = await _dbSet.Where(m => m.Id == userId).Select(m => m.UserName).FirstOrDefaultAsync() ?? "Unknown user";
        return userName;
    }
    public async Task<string> GetUserPictureByIdAsync(string userId)
    {
        var userPicture = await _dbSet.Where(m => m.Id == userId).Select(m => m.Profile.ProfilePicture).FirstOrDefaultAsync() ?? "No Picture Found";
        return userPicture;
    }
}
