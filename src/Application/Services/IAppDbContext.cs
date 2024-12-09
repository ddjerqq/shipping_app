using Domain.Aggregates;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Application.Services;

public interface IAppDbContext : IDisposable
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<Race> Races => Set<Race>();

    public DbSet<TEntity> Set<TEntity>() where TEntity : class;

    public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

    public Task<int> SaveChangesAsync(CancellationToken ct = default);
}