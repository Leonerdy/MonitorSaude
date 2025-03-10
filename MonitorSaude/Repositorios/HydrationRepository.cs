using MonitorSaude.Interfaces;
using MonitorSaude.Models;
using SQLite;

namespace MonitorSaude.Repositorios;
public class HydrationRepository : IHydrationRepository
{
    private readonly SQLiteAsyncConnection _database;

    public HydrationRepository(SQLiteAsyncConnection database)
    {
        _database = database;
    }

    public async Task InsertAsync(HydrationData entity)
    {
        await _database.InsertAsync(entity);
    }

    public async Task<List<HydrationData>> GetAllAsync()
    {
        return await _database.Table<HydrationData>().ToListAsync();
    }
}

