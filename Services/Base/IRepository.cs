namespace Chat_App.Services.Base;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity> LoadAsync(int id);
    Task<IEnumerable<TEntity>> LoadAllAsync();
    Task<bool> SaveAsync(TEntity entity);
    Task<bool> UpdateAsync(TEntity entity);
    Task<bool> DeleteAsync(TEntity entity);
}
