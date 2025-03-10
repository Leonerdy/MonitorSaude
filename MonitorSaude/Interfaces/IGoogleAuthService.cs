namespace MonitorSaude.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<string?> AuthenticateAsync();
    }
}
