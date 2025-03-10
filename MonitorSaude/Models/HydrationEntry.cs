using System;

namespace MonitorSaude.Models
{
    public class HydrationEntry
    {
        public DateTime Timestamp { get; set; } // Data e hora do registro
        public double VolumeInLiters { get; set; } // Volume de água consumido em litros
    }
}
