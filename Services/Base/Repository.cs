﻿
using Chat_App.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat_App.Services.Base;
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<TEntity> _dbSet;
    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }
    public async Task<bool> DeleteAsync(TEntity entity)
    {
        _dbSet.Remove(entity);
        return await SaveChangesAsync();
    }

    public async Task<IEnumerable<TEntity>> LoadAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<TEntity> LoadAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<bool> SaveAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        return await SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        return await SaveChangesAsync();
    }
    private async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

}
