using MonitorSaude.Interfaces;
using MonitorSaude.Models;
using SQLite;

namespace MonitorSaude.Repositorios;

public class CaloriesRepository : ICaloriesRepository
{
    private readonly SQLiteAsyncConnection _database;

    public CaloriesRepository(SQLiteAsyncConnection database)
    {
        _database = database;
    }

    public async Task InsertAsync(CaloriesData entity)
    {
        await _database.InsertAsync(entity);
    }

    public async Task<List<CaloriesData>> GetAllAsync()
    {
        return await _database.Table<CaloriesData>().ToListAsync();
    }
}
