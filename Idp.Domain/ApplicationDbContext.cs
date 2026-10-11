using Idp.Domain.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Idp.Domain;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<SwiyuIdentity> SwiyuIdentity => Set<SwiyuIdentity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure SwiyuIdentity
        builder.Entity<SwiyuIdentity>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(b => b.Id).ValueGeneratedOnAdd();
        });
    }
}
