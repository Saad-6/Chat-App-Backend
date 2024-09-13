using Chat_App.Data;
using Chat_App.Models;
using Chat_App.Services.Base;
using Microsoft.EntityFrameworkCore;

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
        var result = await SaveAsync(user); 
        return result;
    }
    public async Task<bool> UserExists(string email = "", string phone = "")
    {
        return await _dbSet.AnyAsync(u => u.Email == email || u.PhoneNumber == phone);
    }
    public async Task<bool> UpdateUserAsync(User user)
    {
        var result = await UpdateAsync(user);
        return result;
    }
    public async Task<User> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Id == id);
    }
    public async Task<User> GetByPhoneAsync(string phone)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
    }

}
