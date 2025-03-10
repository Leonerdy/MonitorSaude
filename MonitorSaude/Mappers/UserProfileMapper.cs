using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Fitness.v1.Data;
using MonitorSaude.Models;
using Microsoft.Extensions.Logging;
using MonitorSaude.Interfaces;
using Google.Apis.Fitness.v1;

namespace MonitorSaude.Mappers
{
    public class UserProfileMapper : IUserProfileMapper
    {
        private readonly ILogger<UserProfileMapper> _logger;

        public UserProfileMapper(ILogger<UserProfileMapper> logger)
        {
            _logger = logger;
        }

        public async Task<UserProfile> Map(ListDataSourcesResponse response, FitnessService fitnessService)
        {
            if (response == null)
            {
                _logger.LogWarning("Resposta do Google Fit veio nula.");
                return null;
            }

            var userProfile = new UserProfile();
            var dataSources = response.DataSource?.Where(ds => ds.DataType.Name.Contains("com.google"));

            if (dataSources == null || !dataSources.Any())
            {
                _logger.LogWarning("Nenhuma fonte de dados relevante encontrada no Google Fit.");
                return userProfile;
            }

            foreach (var source in dataSources)
            {
                switch (source.DataType.Name)
                {
                    case "com.google.height":
                        await FetchDataPoints(fitnessService, source, userProfile.Heights);
                        break;
                    case "com.google.weight":
                        await FetchDataPoints(fitnessService, source, userProfile.Weights);
                        break;
                }
            }

            _logger.LogInformation("Perfil do usuário mapeado com sucesso.");
            return userProfile;
        }

        private async Task FetchDataPoints(FitnessService fitnessService, DataSource source, List<MeasurementEntry> dataList)
        {
            var dataPointsRequest = fitnessService.Users.DataSources.DataPointChanges.List("me", source.DataStreamId);
            var dataPointsResponse = await dataPointsRequest.ExecuteAsync();

            if (dataPointsResponse?.InsertedDataPoint != null)
            {
                foreach (var dataPoint in dataPointsResponse.InsertedDataPoint)
                {
                    var value = dataPoint.Value.FirstOrDefault();
                    if (value?.FpVal.HasValue == true)
                    {
                        // Converter startTimeNanos para DateTime
                        if (long.TryParse(dataPoint.StartTimeNanos?.ToString(), out long startTimeNanos))
                        {
                            DateTime timestamp = DateTimeOffset.FromUnixTimeMilliseconds(startTimeNanos / 1_000_000).DateTime;

                            dataList.Add(new MeasurementEntry
                            {
                                Timestamp = timestamp,
                                Value = value.FpVal.Value
                            });
                        }
                    }
                }
            }
        }
    }
}
