using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel;

public class MpmDbContext(DbContextOptions<MpmDbContext> options) : IdentityDbContext<MpmUser>(options)
{
    public DbSet<MpmGroup> Groups { get; set; } = null!;
    public DbSet<UserGroupEntry> UserGroupEntries { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}