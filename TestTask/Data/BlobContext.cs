using Microsoft.EntityFrameworkCore;

using TestTask.Model;

namespace TestTask.Data;

public class BlobContext : DbContext
{
    public BlobContext(DbContextOptions<BlobContext> options) : base(options)
    {
    }

    public DbSet<Uploads> Uploads { get; set; }
}