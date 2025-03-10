using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Fitness.v1.Data;
using MonitorSaude.Models;

namespace MonitorSaude.Mappers
{
    public static class HydrationMapper
    {
        public static List<HydrationEntry> MapHydrationData(Dataset dataset)
        {
            var hydrationEntries = new List<HydrationEntry>();

            if (dataset?.Point == null || !dataset.Point.Any())
                return hydrationEntries;

            foreach (var dataPoint in dataset.Point)
            {
                var timestamp = FromNanosecondsToDateTime(dataPoint.StartTimeNanos);

                // Obtendo o valor da hidratação em litros
                var hydrationValueInLiters = dataPoint.Value?.FirstOrDefault()?.FpVal ?? 0;

                // Convertendo para mililitros
                var hydrationValueInMilliliters = hydrationValueInLiters * 1000;

                // Arredondando para o valor inteiro mais próximo
                var roundedHydrationValue = Math.Round(hydrationValueInMilliliters);

                hydrationEntries.Add(new HydrationEntry
                {
                    Timestamp = timestamp,
                    VolumeInLiters = (int)roundedHydrationValue // Armazenando o valor arredondado como inteiro
                });
            }

            return hydrationEntries;
        }

        private static DateTime FromNanosecondsToDateTime(long? nanoseconds)
        {
            long ticks = (long)(nanoseconds / 100); // Convertendo nanosegundos para ticks do .NET
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(ticks);
        }
    }
}
