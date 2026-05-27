using Microsoft.EntityFrameworkCore;

using GrpcCrudDemo.Models;

namespace GrpcCrudDemo.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Person> Persons => Set<Person>();
}