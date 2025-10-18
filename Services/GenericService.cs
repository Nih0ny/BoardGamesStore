using System.Linq.Expressions;
using BoardGamesStore.Data;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services;

public class GenericService<T> : IGenericService<T> where T : class
{
  protected readonly ApplicationDbContext _context;
  protected readonly DbSet<T> _dbSet;

  public GenericService(ApplicationDbContext context)
  {
    _context = context;
    _dbSet = _context.Set<T>();
  }

  public async Task<IEnumerable<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>>? include = null)
  {
    IQueryable<T> query = _dbSet;
    if (include != null)
      query = include(query);

    return await query.ToListAsync();
  }

  public async Task<T?> GetByIdAsync(int id, Func<IQueryable<T>, IQueryable<T>>? include = null)
  {
    IQueryable<T> query = _dbSet;
    if (include != null)
      query = include(query);

    return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
  }

  public async Task AddAsync(T entity)
  {
    await _dbSet.AddAsync(entity);
    await _context.SaveChangesAsync();
  }

  public async Task UpdateAsync(T entity)
  {
    _dbSet.Update(entity);
    await _context.SaveChangesAsync();
  }

  public async Task DeleteAsync(int id)
  {
    var entity = await _dbSet.FindAsync(id);
    if (entity != null)
    {
      _dbSet.Remove(entity);
      await _context.SaveChangesAsync();
    }
  }
}
