using Assignly.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Infrastructure.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<T>();
    }

    public IQueryable<T> Query() => _dbSet.AsQueryable();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _dbSet.FindAsync([id], ct).AsTask();

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await _dbSet.AddAsync(entity, ct);

    public void Remove(T entity) => _dbSet.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _dbContext.SaveChangesAsync(ct);
}
