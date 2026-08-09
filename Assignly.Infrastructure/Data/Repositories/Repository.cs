using Assignly.Application.Interfaces;
using Assignly.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Infrastructure.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _db;

    public Repository(ApplicationDbContext db)
    {
        _db = db;
    }

    public IQueryable<T> Query() => _db.Set<T>().AsQueryable();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Set<T>().FindAsync([id], ct).AsTask();

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await _db.Set<T>().AddAsync(entity, ct);

    public void Remove(T entity) => _db.Set<T>().Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
