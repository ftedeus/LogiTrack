using Microsoft.EntityFrameworkCore;
using LogiTrack.Models;
using System.Collections.Generic;

public class LogiTrackContext : DbContext
{
    public LogiTrackContext(DbContextOptions<LogiTrackContext> options)
        : base(options)
    {
    }

    // Parameterless constructor for design-time tools
    public LogiTrackContext()
    {
        OnConfiguring(new DbContextOptionsBuilder<LogiTrackContext>());
    }

    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=logitrack.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItem>().ToTable("InventoryItems");
        modelBuilder.Entity<Order>().ToTable("Orders");
    }
}

