using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MonitorSaude.Interfaces;
using MonitorSaude.Models;
using Microcharts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonitorSaude.ViewModels
{
    public partial class NutritionViewModel : ObservableObject
    {
        private readonly IGoogleFitService _googleFitService;

        [ObservableProperty]
        private HealthData healthData;

        [ObservableProperty]
        private Chart hydrationChart;

        [ObservableProperty]
        private Chart nutritionChart;

        [ObservableProperty]
        private bool isLoading; // Controla o loader da tela inteira

        [ObservableProperty]
        private bool hasData;

        partial void OnHasDataChanged(bool value)
        {
            OnPropertyChanged(nameof(NoData)); // Notifica que NoData mudou
        }

        public bool NoData => !HasData;

        public ICommand LoadHealthDataCommand { get; private set; }

        public NutritionViewModel(IGoogleFitService googleFitService)
        {
            _googleFitService = googleFitService;
            LoadHealthDataCommand = new AsyncRelayCommand(() => _ = LoadHealthDataAsync());
            _ = LoadHealthDataAsync();
        }

        public async Task LoadHealthDataAsync()
        {
            IsLoading = true;
            HasData = false;

            try
            {
                var healthDataResponse = await _googleFitService.GetHealthDataAsync();
                HealthData = healthDataResponse ?? new HealthData();

                if (healthDataResponse != null &&
                    (healthDataResponse.HydrationEntries.Any() || healthDataResponse.NutritionEntries.Any()))
                {
                    HydrationChart = GenerateHydrationChart(healthDataResponse.HydrationEntries);
                    NutritionChart = GenerateNutritionChart(healthDataResponse.NutritionEntries);
                    HasData = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar dados de saúde: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Chart GenerateHydrationChart(List<HydrationEntry> hydrationEntries)
        {
            if (hydrationEntries == null || !hydrationEntries.Any())
                return null; // Retorna nulo para indicar ausência de dados

            var hydrationByDay = hydrationEntries
                .GroupBy(entry => entry.Timestamp.Date)
                .Select(group => new
                {
                    Day = group.Key,
                    FormattedDate = group.Key.ToString("dd/MM", CultureInfo.InvariantCulture), // Formato: 24/02
                    DayOfWeek = CultureInfo.GetCultureInfo("pt-BR").DateTimeFormat.GetDayName(group.Key.DayOfWeek),
                    TotalVolume = group.Sum(entry => entry.VolumeInLiters)
                })
                .OrderBy(entry => entry.Day)
                .ToList();

            var hydrationChartEntries = hydrationByDay.Select(entry => new ChartEntry((float)entry.TotalVolume)
            {
                Label = $"{entry.FormattedDate} ({entry.DayOfWeek.Substring(0, 3)})", // Exemplo: 24/02 (Seg)
                ValueLabel = entry.TotalVolume.ToString("F1"),
                Color = SKColor.Parse("#3498db")
            }).ToList();

            return new LineChart { Entries = hydrationChartEntries };
        }

        private Chart GenerateNutritionChart(List<NutritionEntry> nutritionEntries)
        {
            if (nutritionEntries == null || !nutritionEntries.Any())
                return null;

            var nutritionByDay = nutritionEntries
                .GroupBy(entry => entry.Timestamp.Date)
                .Select(group => new
                {
                    Day = group.Key,
                    FormattedDate = group.Key.ToString("dd/MM", CultureInfo.InvariantCulture), // Formato: 24/02
                    DayOfWeek = CultureInfo.GetCultureInfo("pt-BR").DateTimeFormat.GetDayName(group.Key.DayOfWeek),
                    TotalCalories = group.Sum(entry => entry.Calories)
                })
                .OrderBy(entry => entry.Day)
                .ToList();

            var nutritionChartEntries = nutritionByDay.Select(entry => new ChartEntry((float)entry.TotalCalories)
            {
                Label = $"{entry.FormattedDate} ({entry.DayOfWeek.Substring(0, 3)})", // Exemplo: 24/02 (Seg)
                ValueLabel = entry.TotalCalories.ToString("F1"),
                Color = SKColor.Parse("#2ecc71")
            }).ToList();

            return new LineChart { Entries = nutritionChartEntries };
        }
    }
}


