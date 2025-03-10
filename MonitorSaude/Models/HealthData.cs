using CommunityToolkit.Mvvm.ComponentModel;

namespace MonitorSaude.Models
{
    public partial class HealthData : ObservableObject
    {
    
        [ObservableProperty]
        private List<HydrationEntry> hydrationEntries;
        [ObservableProperty]
        private List<NutritionEntry> nutritionEntries;

        public HealthData()
        {
            hydrationEntries = new List<HydrationEntry>();
            nutritionEntries = new List<NutritionEntry>();
        }
    }
}
