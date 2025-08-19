using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;
using SharedLibrary.Models.Database;

namespace SharedLibrary.Data;

public class AppDbContext : DbContext
{
    public DbSet<CharacterDto> Characters { get; set; }
    public DbSet<EveSystemDto> EveSystems { get; set; }
    
    
    public DbSet<DungeonDto> Dungeons { get; set; }
    public DbSet<DungeonLevelDto> DungeonLevels { get; set; }
    public DbSet<FactionDto> Factions { get; set; }
    public DbSet<DungeonFactionDto> DungeonFactions { get; set; }
    
    public DbSet<SettingDto> Settings { get; set; }
    
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
        
        // Settings
        modelBuilder.Entity<SettingDto>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Value).HasMaxLength(500).IsRequired();
            entity.Property(e => e.LastUpdated).IsRequired(false);
        });
        
        // DUNGEON
        modelBuilder.Entity<DungeonDto>(entity =>
        {
            entity.HasIndex(e => e.Type);
        });

        // DUNGEON LEVEL
        modelBuilder.Entity<DungeonLevelDto>(entity =>
        {
            entity.HasIndex(e => e.Level); 

            entity.HasOne(e => e.Dungeon)
                .WithMany(d => d.Levels)
                .HasForeignKey(e => e.DungeonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // FACTION
        modelBuilder.Entity<FactionDto>(entity =>
        {
            entity.HasIndex(e => e.Name);
        });

        // DUNGEON FACTION
        modelBuilder.Entity<DungeonFactionDto>(entity =>
        {
            entity.HasKey(e => new { e.DungeonId, e.FactionId }); // Composite PK

            entity.HasOne(df => df.Dungeon)
                .WithMany(d => d.Factions)
                .HasForeignKey(df => df.DungeonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(df => df.Faction)
                .WithMany(f => f.DungeonLinks)
                .HasForeignKey(df => df.FactionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(df => df.FactionId);
        });
        
    }

}