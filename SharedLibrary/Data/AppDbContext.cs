using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;
using SharedLibrary.Models.Database;

namespace SharedLibrary.Data;

public class AppDbContext : DbContext
{
    public DbSet<CharacterDto> Characters { get; set; }
    public DbSet<EveSystemDto> EveSystems { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=eve_tracker.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CharacterDto>()
            .HasKey(c => c.CharacterId);
        modelBuilder.Entity<CharacterDto>().Property(x => x.CharacterId).ValueGeneratedNever();
        
        modelBuilder.Entity<EveSystemDto>()
            .HasKey(e => e.SystemId);
        modelBuilder.Entity<EveSystemDto>().Property(x => x.SystemId).ValueGeneratedNever();
        
    }

}