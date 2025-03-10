using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MonitorSaude.Interfaces;
using MonitorSaude.Models;
using Microcharts;
using SkiaSharp;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonitorSaude.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IGoogleFitService _googleFitService;

        [ObservableProperty]
        private string authenticationStatus;

        [ObservableProperty]
        private UserProfile userData;

        [ObservableProperty]
        private Chart weightChart;

        [ObservableProperty]
        private Chart heightChart;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool hasData;

        partial void OnHasDataChanged(bool value)
        {
            OnPropertyChanged(nameof(NoData));
        }

        public bool NoData => !HasData;

        public ICommand AuthenticateAsyncCommand { get; private set; }
        public ICommand LoadUserDataCommand { get; private set; }
        public ICommand InsertUserDataCommand { get; }

        public MainViewModel(IGoogleAuthService googleAuthService, IGoogleFitService googleFitService)
        {
            _googleAuthService = googleAuthService;
            _googleFitService = googleFitService;
            AuthenticationStatus = "Aguardando autenticação...";
            AuthenticateAsyncCommand = new AsyncRelayCommand(() => _ = AuthenticateAsync());
            LoadUserDataCommand = new AsyncRelayCommand(() => _ = LoadUserDataAsync());
            InsertUserDataCommand = new AsyncRelayCommand(() => InsertUserDataAsync());
            _ = AuthenticateAsync();
        }

        public async Task AuthenticateAsync()
        {
            try
            {
                AuthenticationStatus = "Autenticando...";
                var accessToken = await _googleAuthService.AuthenticateAsync();
                AuthenticationStatus = accessToken != null ? "Autenticação bem-sucedida!" : "Falha na autenticação.";

                if (!string.IsNullOrEmpty(accessToken))
                {
                    var isInitialized = await _googleFitService.InitializeAsync(accessToken);
                    if (isInitialized)
                    {
                        await LoadUserDataAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                AuthenticationStatus = $"Erro: {ex.Message}";
                Console.WriteLine($"Erro na autenticação: {ex}");
            }
        }
        public async Task InsertUserDataAsync()
        {
            if (double.TryParse(UserData.Weights[0].Value.ToString(), out double weight) &&
                double.TryParse(UserData.Heights[0].Value.ToString(), out double height))
            {
                var success = await _googleFitService.InsertUserDataAsync(weight, height);
                if (success)
                {
                    await LoadUserDataAsync(); // Atualiza os dados exibidos
                }
            }
        }
        public async Task LoadUserDataAsync()
        {
            IsLoading = true;
            HasData = false;

            try
            {
                var userProfile = await _googleFitService.GetUserProfileAsync();
                UserData = userProfile ?? new UserProfile();

                if (userProfile != null && (userProfile.Weights.Any() || userProfile.Heights.Any()))
                {
                    WeightChart = GenerateWeightChart(userProfile.Weights);
                    HeightChart = GenerateHeightChart(userProfile.Heights);
                    HasData = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar dados do usuário: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Chart GenerateWeightChart(List<MeasurementEntry> weightEntries)
        {
            if (weightEntries == null || !weightEntries.Any())
                return null;

            var weightChartEntries = weightEntries.Select(entry => new ChartEntry((float)entry.Value)
            {
                Label = entry.Timestamp.ToString("dd/MM", CultureInfo.InvariantCulture),
                ValueLabel = entry.Value.ToString("F1"),
                Color = SKColor.Parse("#3498db")
            }).ToList();

            return new LineChart { Entries = weightChartEntries };
        }

        private Chart GenerateHeightChart(List<MeasurementEntry> heightEntries)
        {
            if (heightEntries == null || !heightEntries.Any())
                return null;

            var heightChartEntries = heightEntries.Select(entry => new ChartEntry((float)entry.Value)
            {
                Label = entry.Timestamp.ToString("dd/MM", CultureInfo.InvariantCulture),
                ValueLabel = entry.Value.ToString("F2"),
                Color = SKColor.Parse("#2ecc71")
            }).ToList();

            return new BarChart { Entries = heightChartEntries };

        }
    }
}
