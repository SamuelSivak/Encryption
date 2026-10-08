using Encryption.Models.SQL;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Encryption.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<EncryptedLinkMeta> EncryptedLinkMetas => Set<EncryptedLinkMeta>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        
        builder.Entity<EncryptedLinkMeta>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.ShortCode)
                  .IsUnique();

            entity.HasIndex(e => e.BlindIndexHash);

            entity.HasIndex(e => e.OwnerUserId);
        });
    }
}