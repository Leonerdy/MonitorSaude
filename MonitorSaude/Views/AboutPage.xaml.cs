using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace MonitorSaude.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private async void OnLearnMoreClicked(object sender, EventArgs e)
        {
            var url = "https://www.auvo.com"; // Substituir pelo link oficial, se houver
            try
            {
                await Launcher.OpenAsync(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao abrir link: {ex.Message}");
            }
        }
    }
}
