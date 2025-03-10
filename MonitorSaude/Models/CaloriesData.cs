using SQLite;

namespace MonitorSaude.Models
{
    public class CaloriesData
    {
        [PrimaryKey, AutoIncrement]
        public int CaloriesnId { get; set; }
        public double Calorias { get; set; }
        public DateTime Timestamp { get; set; }

    }
}
