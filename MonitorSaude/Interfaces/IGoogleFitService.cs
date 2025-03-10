using System.Threading.Tasks;
using MonitorSaude.Models;

namespace MonitorSaude.Interfaces;

    public interface IGoogleFitService
    {
        Task<bool> InitializeAsync(string accessToken);
        Task<UserProfile> GetUserProfileAsync();
        Task<HealthData> GetHealthDataAsync();
        Task<bool> InsertUserDataAsync(double weight, double height);
        Task<bool> InsertHydrationDataAsync(double hidratacao);
        Task<bool> InsertCaloriesDataAsync(double calorias);
}
