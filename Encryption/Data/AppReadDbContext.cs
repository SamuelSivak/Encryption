using Encryption.Models.SQL;
using Microsoft.EntityFrameworkCore;

namespace Encryption.Data;

public class AppReadDbContext : DbContext
{
    public AppReadDbContext(DbContextOptions<AppReadDbContext> options) : base(options)
    {
    }

    public DbSet<EncryptedLinkMeta> EncryptedLinkMetas => Set<EncryptedLinkMeta>();
}