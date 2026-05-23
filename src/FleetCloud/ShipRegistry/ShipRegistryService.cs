using Microsoft.EntityFrameworkCore;

namespace CargoShipMonitoring.FleetCloud.ShipRegistry;

public class ShipRegistryService : IShipRegistryService
{
    private readonly string _dbPath;

    public ShipRegistryService(string dbPath)
    {
        _dbPath = dbPath;

        using var db = CreateContext();
        db.Database.EnsureCreated();
    }

    public async Task RegisterAsync(string shipId, string name, string? imoNumber = null)
    {
        using var db = CreateContext();

        var existing = await db.Ships.FindAsync(shipId);
        if (existing != null)
        {
            existing.Name = name;
            existing.IMONumber = imoNumber;
        }
        else
        {
            db.Ships.Add(new ShipInfo
            {
                ShipId = shipId,
                Name = name,
                IMONumber = imoNumber
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task<ShipInfo?> GetAsync(string shipId)
    {
        using var db = CreateContext();
        return await db.Ships.FindAsync(shipId);
    }

    public async Task UpdateLastSeenAsync(string shipId)
    {
        using var db = CreateContext();

        var ship = await db.Ships.FindAsync(shipId);
        if (ship != null)
        {
            ship.LastSeen = DateTimeOffset.UtcNow;
            ship.Status = "Online";
            await db.SaveChangesAsync();
        }
    }

    public async Task UpdateStatusAsync(string shipId, string status)
    {
        using var db = CreateContext();

        var ship = await db.Ships.FindAsync(shipId);
        if (ship != null)
        {
            ship.Status = status;
            if (status == "Offline")
                ship.LastSeen = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<ShipInfo>> GetAllAsync()
    {
        using var db = CreateContext();
        return await db.Ships.ToListAsync();
    }

    private RegistryDbContext CreateContext() => new(_dbPath);
}

public class RegistryDbContext : DbContext
{
    private readonly string _dbPath;

    public DbSet<ShipInfo> Ships => Set<ShipInfo>();

    public RegistryDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShipInfo>(entity =>
        {
            entity.HasKey(e => e.ShipId);
            entity.HasIndex(e => e.Status);
        });
    }
}
