using Google.Apis.Fitness.v1.Data;
using MonitorSaude.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonitorSaude.Mappers
{
    public static class NutritionMapper
    {
        public static List<NutritionEntry> MapNutritionData(Dataset dataset)
        {
            var nutritionEntries = new List<NutritionEntry>();

            if (dataset?.Point == null || !dataset.Point.Any())
                return nutritionEntries;

            foreach (var dataPoint in dataset.Point)
            {
                var timestamp = FromNanosecondsToDateTime(dataPoint.StartTimeNanos);

                var foodName = dataPoint.Value?.ElementAtOrDefault(2)?.StringVal ?? "Desconhecido";
                var calories = dataPoint.Value?.ElementAtOrDefault(0)?.MapVal.FirstOrDefault().Value.FpVal ?? 0;

                nutritionEntries.Add(new NutritionEntry
                {
                    Timestamp = timestamp,
                    FoodName = foodName,
                    Calories = calories
                });
            }

            return nutritionEntries;
        }

        private static DateTime FromNanosecondsToDateTime(long? nanoseconds)
        {
            long ticks = (long)(nanoseconds / 100); // Convertendo nanosegundos para ticks do .NET
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(ticks);
        }
    }
}
