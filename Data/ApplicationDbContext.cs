using Microsoft.EntityFrameworkCore;
using AvaloniaProject.Models;

namespace AvaloniaProject.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Game> Games { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=project.db");
    }
    
}