using Chat_App.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chat_App.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users {  get; set; }
    public DbSet<Message> Messages {  get; set; }
    public DbSet<Chat> Chats {  get; set; }
    public DbSet<Profile> Profiles {  get; set; }
    public DbSet<ContactList> ContactLists {  get; set; }

  
}
