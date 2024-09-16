
using Chat_App.Data;
using Microsoft.EntityFrameworkCore;
using Chat_App.Code;
using Chat_App.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq.Expressions;
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
        if(Utility.Null(entity)) return false;
        _dbSet.Remove(entity);
        return await SaveChangesAsync();
    }

    public async Task<IEnumerable<TEntity>> LoadAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<TEntity> LoadByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    public async Task<List<TEntity>> GetRequestBySearchParameterAsync(string propertyName, object searchparameter)
    {
        // Get the parameter expression representing 'm'
        var parameter = Expression.Parameter(typeof(TEntity), "m");

        // Get the property 'm.propertyName'
        var property = Expression.Property(parameter, propertyName);

        // Create the constant expression for the search parameter
        var constant = Expression.Constant(searchparameter);

        // Create the equality expression 'm.propertyName == searchParameter'
        var equalExpression = Expression.Equal(property, constant);

        // Compile the expression into a lambda 'm => m.propertyName == searchParameter'
        var lambda = Expression.Lambda<Func<TEntity, bool>>(equalExpression, parameter);
        return await _dbSet.Where(lambda).ToListAsync();
    }

    public async Task<bool> SaveAsync(TEntity entity)
    {
        if (Utility.Null(entity)) return false;
        await _dbSet.AddAsync(entity);
        return await SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(TEntity entity)
    {
        if (Utility.Null(entity)) return false;
        _dbSet.Update(entity);
        return await SaveChangesAsync();
    }
    private async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
   
}
