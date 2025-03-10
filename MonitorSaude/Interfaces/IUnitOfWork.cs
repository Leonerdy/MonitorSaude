namespace MonitorSaude.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserDataRepository UserDataRepository { get; }
    IHydrationRepository HydrationRepository { get; }
    ICaloriesRepository CaloriesRepository { get; }
    Task InitializeDatabaseAsync();
}

