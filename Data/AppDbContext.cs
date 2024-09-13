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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Contacts)
            .WithMany(c => c.Contacts);

        modelBuilder.Entity<ContactList>()
            .HasMany(c => c.Contacts)
            .WithOne(u => u.Contacts);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User);
    }
}
