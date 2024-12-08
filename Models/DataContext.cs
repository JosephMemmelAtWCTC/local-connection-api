using Microsoft.EntityFrameworkCore;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    protected virtual CancellationToken CancellationToken => CancellationToken.None;

    public required DbSet<LocalLocation> LocalLocations { get; set; }

    public virtual LocalLocation? FindById(string localLocationId)
    {
        // ThrowIfDisposed();
        return LocalLocations.Find(localLocationId);//, CancellationToken);
    }
}
