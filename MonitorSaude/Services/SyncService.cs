using Microsoft.Extensions.Logging;
using MonitorSaude.Interfaces;

namespace MonitorSaude.Services;
public class SyncService : ISyncService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGoogleFitService _googleFitService;
    private readonly ILogger<SyncService> _logger;

    public SyncService(IUnitOfWork unitOfWork, IGoogleFitService googleFitService, ILogger<SyncService> logger)
    {
        _unitOfWork = unitOfWork;
        _googleFitService = googleFitService;
        _logger = logger;
    }

    public async Task SyncDataAsync()
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            _logger.LogInformation("Sincronizando dados com Google Fit...");

            // Obtém os dados locais
            var localUsers = await _unitOfWork.UserDataRepository.GetAllAsync();
            var localHydration = await _unitOfWork.HydrationRepository.GetAllAsync();
            var localCalories = await _unitOfWork.CaloriesRepository.GetAllAsync();

            // Envia os dados para o Google Fit
            foreach (var user in localUsers)
            {
                await _googleFitService.InsertUserDataAsync(user.Weight, user.Height);
            }

            foreach (var hydration in localHydration)
            {
                await _googleFitService.InsertHydrationDataAsync(hydration.Hidratacao);
            }

            foreach (var calories in localCalories)
            {
                await _googleFitService.InsertCaloriesDataAsync(calories.Calorias);
            }

            _logger.LogInformation("Sincronização concluída!");
        }
        else
        {
            _logger.LogWarning("Sem conexão. Dados permanecerão offline.");
        }
    }
}

