using Microsoft.EntityFrameworkCore;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public required DbSet<LocalLocation> LocalLocations { get; set; }
}
