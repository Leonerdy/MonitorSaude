using MonitorSaude.Models;
using MonitorSaude.Utils;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SQLiteService
{
    private SQLiteAsyncConnection _database;

    public SQLiteService()
    {
    }

    private async Task InitializeDatabaseAsync()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

        // Criação das tabelas necessárias
        await _database.CreateTableAsync<UserData>();
        await _database.CreateTableAsync<HydrationData>();
        await _database.CreateTableAsync<CaloriesData>();
    }

    public async Task InsertUserDataAsync(UserData data)
    {
        await InitializeDatabaseAsync();
        await _database.InsertAsync(data);
    }

    public async Task<List<UserData>> GetUserDataAsync()
    {
        await InitializeDatabaseAsync();
        return await _database.Table<UserData>().ToListAsync();
    }

    public async Task InsertHydrationDataAsync(HydrationData data)
    {
        await InitializeDatabaseAsync();
        await _database.InsertAsync(data);
    }

    public async Task<List<HydrationData>> GetHydrationDataAsync()
    {
        await InitializeDatabaseAsync();
        return await _database.Table<HydrationData>().ToListAsync();
    }

    public async Task InsertCaloriesDataAsync(CaloriesData data)
    {
        await InitializeDatabaseAsync();
        await _database.InsertAsync(data);
    }

    public async Task<List<CaloriesData>> GetCaloriesDataAsync()
    {
        await InitializeDatabaseAsync();
        return await _database.Table<CaloriesData>().ToListAsync();
    }
}
