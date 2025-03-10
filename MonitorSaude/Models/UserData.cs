using SQLite;

namespace MonitorSaude.Models
{
    public class UserData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
