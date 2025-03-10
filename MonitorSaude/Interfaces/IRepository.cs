namespace MonitorSaude.Interfaces;

public interface IRepository<T> where T : class, new()
{
    Task InsertAsync(T entity);
    Task<List<T>> GetAllAsync();
}

