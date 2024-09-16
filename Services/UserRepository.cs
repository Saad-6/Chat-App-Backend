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
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> GetByIdAsync(string id)
    {
        if (Utility.Null(id)) return null;
        return await _dbSet
        .Include(u => u.Contacts)
        .Include(u=>u.Profile)
        .FirstOrDefaultAsync(u => u.Id == id);
    }
    public async Task<User> GetByPhoneAsync(string phone)
    {
        if (Utility.Null(phone)) return null;
        return await _dbSet.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
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
    public async Task<string> GetUserNameByIdAsync(string userId)
    {
        return await _dbSet.Where(m=>m.Id == userId).Select(m => m.UserName).FirstOrDefaultAsync();
    }
    public async Task<string> GetUserPictureByIdAsync(string userId)
    {
        return await _dbSet.Where(m => m.Id == userId).Select(m => m.Profile.ProfilePicture).FirstOrDefaultAsync();
    }
}
