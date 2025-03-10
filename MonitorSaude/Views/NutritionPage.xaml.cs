using MonitorSaude.ViewModels;

namespace MonitorSaude.Views;

public partial class NutritionPage : ContentPage
{
    public NutritionPage(NutritionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}