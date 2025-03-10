using System;

namespace MonitorSaude.Utils
{
    public class DateUtils
    {
        public static (long startOfLast7Days, long endOfLast7Days) GetWeekTimeRange()
        {
            DateTime now = DateTime.UtcNow;

            // Definir o início como exatamente 7 dias atrás, mantendo o horário zerado (00:00:00)
            DateTime startOfLast7Days = now.Date.AddDays(-7);
            DateTime endOfLast7Days = now.Date.AddDays(1).AddTicks(-1); // Último momento do dia atual

            // Converter para nanossegundos
            long startTimestamp = ToUnixTimestampInNanoseconds(startOfLast7Days);
            long endTimestamp = ToUnixTimestampInNanoseconds(endOfLast7Days);

            return (startTimestamp, endTimestamp);
        }

        public static long ToUnixTimestampInNanoseconds(DateTime dateTime)
        {
            return (dateTime.Ticks - new DateTime(1970, 1, 1).Ticks) * 100;
        }
    }
}


