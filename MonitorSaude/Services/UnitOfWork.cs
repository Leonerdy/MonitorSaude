using MonitorSaude.Interfaces;
using MonitorSaude.Models;
using MonitorSaude.Repositorios;
using MonitorSaude.Utils;
using SQLite;

namespace MonitorSaude.Services;

public class UnitOfWork : IUnitOfWork
{
    private SQLiteAsyncConnection _database;
    public IUserDataRepository UserDataRepository { get; private set; }
    public IHydrationRepository HydrationRepository { get; private set; }
    public ICaloriesRepository CaloriesRepository { get; private set; }

    public UnitOfWork()
    {
        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        UserDataRepository = new UserDataRepository(_database);
        HydrationRepository = new HydrationRepository(_database);
        CaloriesRepository = new CaloriesRepository(_database);
    }

    public async Task InitializeDatabaseAsync()
    {
        await _database.CreateTableAsync<UserData>();
        await _database.CreateTableAsync<HydrationData>();
        await _database.CreateTableAsync<CaloriesData>();
    }

    public void Dispose()
    {
        _database.CloseAsync();
    }
}

