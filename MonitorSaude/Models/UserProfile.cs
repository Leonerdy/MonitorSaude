namespace MonitorSaude.Models
{
    public class UserProfile
    {
        public List<MeasurementEntry> Weights { get; set; } = new List<MeasurementEntry>();
        public List<MeasurementEntry> Heights { get; set; } = new List<MeasurementEntry>();
    }

    public class MeasurementEntry
    {
        public DateTime Timestamp { get; set; }  // Data da medição
        public double Value { get; set; }        // Valor (peso ou altura)
    }
}
