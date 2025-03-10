using MonitorSaude.Interfaces;
using MonitorSaude.Models;
using SQLite;

namespace MonitorSaude.Repositorios;

public class UserDataRepository : IUserDataRepository
{
    private readonly SQLiteAsyncConnection _database;

    public UserDataRepository(SQLiteAsyncConnection database)
    {
        _database = database;
    }

    public async Task InsertAsync(UserData entity)
    {
        await _database.InsertAsync(entity);
    }

    public async Task<List<UserData>> GetAllAsync()
    {
        return await _database.Table<UserData>().ToListAsync();
    }
}

