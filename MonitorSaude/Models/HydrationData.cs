using SQLite;

namespace MonitorSaude.Models
{
    public class HydrationData
    {
        [PrimaryKey, AutoIncrement]
        public int HydrationId { get; set; }
        public double Hidratacao { get; set; }
        public DateTime Timestamp { get; set; }

    }
}
