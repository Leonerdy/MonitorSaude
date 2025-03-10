namespace MonitorSaude.Models
{
    public class NutritionEntry
    {
        public DateTime Timestamp { get; set; } // Data do ponto de nutrição
        public string FoodName { get; set; } // Nome do alimento
        public double Calories { get; set; } // Calorias do alimento
    }
}
