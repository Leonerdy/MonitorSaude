using Google.Apis.Auth.OAuth2;
using Google.Apis.Fitness.v1;
using Google.Apis.Services;
using Google.Apis.Fitness.v1.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using MonitorSaude.Models;
using Microsoft.Extensions.Logging;
using MonitorSaude.Interfaces;
using MonitorSaude.Utils;
using MonitorSaude.Mappers;
using Application = Google.Apis.Fitness.v1.Data.Application;

namespace MonitorSaude.Services
{
    public class GoogleFitService : IGoogleFitService
    {
        private readonly ILogger<GoogleFitService> _logger;
        private readonly IUserProfileMapper _userProfileMapper;
        private FitnessService? _fitnessService;

        public GoogleFitService(ILogger<GoogleFitService> logger, IUserProfileMapper userProfileMapper)
        {
            _logger = logger;
            _userProfileMapper = userProfileMapper;
        }

        public async Task<bool> InitializeAsync(string accessToken)
        {
            try
            {
                var credential = GoogleCredential.FromAccessToken(accessToken);

                _fitnessService = new FitnessService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "MonitorSaude"
                });

                _logger.LogInformation("Google Fit Service inicializado com sucesso.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar Google Fit.");
                return false;
            }
        }

        public async Task<bool> CreateWeightAndHeightDataSourcesAsync()
        {
            try
            {
                var deviceModel = DeviceInfo.Model;
                var deviceManufacturer = DeviceInfo.Manufacturer;
                var deviceType = DeviceInfo.DeviceType;
                var deviceUID = deviceModel + deviceManufacturer + deviceType;
                var id = "1040818615650";
                // DataStreamIds para peso e altura
                var weightDataSourceId = $"raw:com.google.weight:{id}:{deviceManufacturer}:{deviceModel}:{deviceUID}";
                var heightDataSourceId = $"raw:com.google.height:{id}:{deviceManufacturer}:{deviceModel}:{deviceUID}";

                var existingDataSources = await _fitnessService.Users.DataSources.List("me").ExecuteAsync();

                // Verifica se o DataSource de peso já existe
                if (existingDataSources.DataSource.Any(ds => ds.DataStreamId == weightDataSourceId))
                {
                    _logger.LogInformation($"DataSource {weightDataSourceId} já existe. Nenhuma ação necessária.");
                }
                else
                {
                    // Criando DataSource para peso
                    var weightDataSource = new DataSource
                    {
                        DataStreamId = weightDataSourceId,
                        DataType = new DataType { Name = "com.google.weight" },
                        Application = new Application
                        {
                            Name = "MonitorSaude",
                            Version = "1.0"
                        },
                        Device = new Google.Apis.Fitness.v1.Data.Device
                        {
                            Manufacturer = deviceManufacturer,
                            Model = deviceModel,
                            Type = "phone",
                            Uid = deviceUID
                        },
                        Type = "raw"
                    };

                    var weightRequest = _fitnessService.Users.DataSources.Create(weightDataSource, "me");
                    var weightResponse = await weightRequest.ExecuteAsync();
                    _logger.LogInformation($"DataSource de peso criado: {weightResponse.DataStreamId}");
                }

                // Verifica se o DataSource de altura já existe
                if (existingDataSources.DataSource.Any(ds => ds.DataStreamId == heightDataSourceId))
                {
                    _logger.LogInformation($"DataSource {heightDataSourceId} já existe. Nenhuma ação necessária.");
                }
                else
                {
                    // Criando DataSource para altura
                    var heightDataSource = new DataSource
                    {
                        DataStreamId = heightDataSourceId,
                        DataType = new DataType { Name = "com.google.height" },
                        Application = new Application
                        {
                            Name = "MonitorSaude",
                            Version = "1.0"
                        },
                        Device = new Google.Apis.Fitness.v1.Data.Device
                        {
                            Manufacturer = deviceManufacturer,
                            Model = deviceModel,
                            Type = "phone",
                            Uid = deviceUID
                        },
                        Type = "raw"
                    };

                    var heightRequest = _fitnessService.Users.DataSources.Create(heightDataSource, "me");
                    var heightResponse = await heightRequest.ExecuteAsync();
                    _logger.LogInformation($"DataSource de altura criado: {heightResponse.DataStreamId}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar DataSources de peso e altura.");
                return false;
            }
        }



        public async Task<bool> InsertUserDataAsync(double weight, double height)
        {
            if (_fitnessService == null)
            {
                _logger.LogWarning("Google Fit Service não foi inicializado.");
                return false;
            }

            try
            {
                var endTime = DateTime.UtcNow;
                var startTime = endTime.AddMinutes(-1);

                var weightDataSourceId = "raw:com.google.weight:1040818615650:phone:sansung:1040818615650";
                var heightDataSourceId = "raw:com.google.height:1040818615650:phone:sansung:1040818615650";

                // Criar DataSources se não existirem
                await CreateWeightAndHeightDataSourcesAsync();

                // Certifique-se de que o startTime e endTime não estão no futuro
                

                // Calcular em nanossegundos (1 tick = 100 nanossegundos, então multiplicamos por 100)
                var startTimeNanos = DateUtils.ToUnixTimestampInNanoseconds(startTime);
                var endTimeNanos = DateUtils.ToUnixTimestampInNanoseconds(endTime);

                // Criação do ponto de dados de peso
                var weightDataPoint = new DataPoint
                {
                    DataTypeName = "com.google.weight",
                    StartTimeNanos = startTimeNanos,
                    EndTimeNanos = endTimeNanos,
                    Value = new List<Value> { new Value { FpVal = weight } }
                };

                var weightDataSet = new Dataset
                {
                    DataSourceId = weightDataSourceId,
                    Point = new List<DataPoint> { weightDataPoint },
                    MinStartTimeNs = startTimeNanos,
                    MaxEndTimeNs = endTimeNanos
                };

                // Criação do ponto de dados de altura
                var heightDataPoint = new DataPoint
                {
                    DataTypeName = "com.google.height",
                    StartTimeNanos = startTimeNanos,
                    EndTimeNanos = endTimeNanos,
                    Value = new List<Value> { new Value { FpVal = height } }
                };

                var heightDataSet = new Dataset
                {
                    DataSourceId = heightDataSourceId,
                    Point = new List<DataPoint> { heightDataPoint },
                    MinStartTimeNs = startTimeNanos,
                    MaxEndTimeNs = endTimeNanos
                };
                long startOfWeek2, endOfWeek2;
                (startOfWeek2, endOfWeek2) = DateUtils.GetWeekTimeRange();
                string datasetId = $"{startTimeNanos}-{endTimeNanos}";


                // Enviar as requisições para o Google Fit
                var weightRequest = _fitnessService.Users.DataSources.Datasets.Patch(weightDataSet, "me", weightDataSourceId, datasetId);
                var heightRequest = _fitnessService.Users.DataSources.Datasets.Patch(heightDataSet, "me", heightDataSourceId, datasetId);

                await weightRequest.ExecuteAsync();
                await heightRequest.ExecuteAsync();

                _logger.LogInformation("Dados de altura e peso inseridos com sucesso.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inserir dados no Google Fit.");
                return false;
            }
        }
        public async Task<bool> CreateCaloriesDataSourceAsync()
        {
            try
            {
                var deviceModel = DeviceInfo.Model;
                var deviceManufacturer = DeviceInfo.Manufacturer;
                var deviceType = DeviceInfo.DeviceType;
                var deviceUID = $"{deviceModel}{deviceManufacturer}{deviceType}";
                var id = "1040818615650"; // Substituir pelo seu ID correto

                var caloriesDataSourceId = $"raw:com.google.calories.expended:{id}:{deviceManufacturer}:{deviceModel}:{deviceUID}";

                var existingDataSources = await _fitnessService.Users.DataSources.List("me").ExecuteAsync();

                if (existingDataSources.DataSource.Any(ds => ds.DataStreamId == caloriesDataSourceId))
                {
                    _logger.LogInformation($"DataSource {caloriesDataSourceId} já existe.");
                    return true;
                }

                var caloriesDataSource = new DataSource
                {
                    DataStreamId = caloriesDataSourceId,
                    DataType = new DataType { Name = "com.google.calories.expended" },
                    Application = new Application
                    {
                        Name = "MonitorSaude",
                        Version = "1.0"
                    },
                    Device = new Google.Apis.Fitness.v1.Data.Device
                    {
                        Manufacturer = deviceManufacturer,
                        Model = deviceModel,
                        Type = "phone",
                        Uid = deviceUID
                    },
                    Type = "raw"
                };

                var caloriesRequest = _fitnessService.Users.DataSources.Create(caloriesDataSource, "me");
                var caloriesResponse = await caloriesRequest.ExecuteAsync();
                _logger.LogInformation($"DataSource de calorias criado: {caloriesResponse.DataStreamId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar DataSource de calorias.");
                return false;
            }
        }
        public async Task<bool> CreateHydrationDataSourceAsync()
        {
            try
            {
                var deviceModel = DeviceInfo.Model;
                var deviceManufacturer = DeviceInfo.Manufacturer;
                var deviceType = DeviceInfo.DeviceType;
                var deviceUID = $"{deviceModel}{deviceManufacturer}{deviceType}";
                var id = "1040818615650"; // Substituir pelo seu ID correto

                var hydrationDataSourceId = $"raw:com.google.hydration:{id}:{deviceManufacturer}:{deviceModel}:{deviceUID}";

                var existingDataSources = await _fitnessService.Users.DataSources.List("me").ExecuteAsync();

                if (existingDataSources.DataSource.Any(ds => ds.DataStreamId == hydrationDataSourceId))
                {
                    _logger.LogInformation($"DataSource {hydrationDataSourceId} já existe.");
                    return true;
                }

                var hydrationDataSource = new DataSource
                {
                    DataStreamId = hydrationDataSourceId,
                    DataType = new DataType { Name = "com.google.hydration" },
                    Application = new Application
                    {
                        Name = "MonitorSaude",
                        Version = "1.0"
                    },
                    Device = new Google.Apis.Fitness.v1.Data.Device
                    {
                        Manufacturer = deviceManufacturer,
                        Model = deviceModel,
                        Type = "phone",
                        Uid = deviceUID
                    },
                    Type = "raw"
                };

                var hydrationRequest = _fitnessService.Users.DataSources.Create(hydrationDataSource, "me");
                var hydrationResponse = await hydrationRequest.ExecuteAsync();
                _logger.LogInformation($"DataSource de hidratação criado: {hydrationResponse.DataStreamId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar DataSource de hidratação.");
                return false;
            }
        }


        public async Task<bool> InsertCaloriesDataAsync(double caloriesBurned)
        {
            if (_fitnessService == null)
            {
                _logger.LogWarning("Google Fit Service não foi inicializado.");
                return false;
            }

            try
            {
                var endTime = DateTime.UtcNow;
                var startTime = endTime.AddMinutes(-1);
                var caloriesDataSourceId = "raw:com.google.calories.expended:1040818615650:phone:samsung:1040818615650";

                await CreateCaloriesDataSourceAsync();

                var startTimeNanos = DateUtils.ToUnixTimestampInNanoseconds(startTime);
                var endTimeNanos = DateUtils.ToUnixTimestampInNanoseconds(endTime);
                string datasetId = $"{startTimeNanos}-{endTimeNanos}";

                var caloriesDataPoint = new DataPoint
                {
                    DataTypeName = "com.google.calories.expended",
                    StartTimeNanos = startTimeNanos,
                    EndTimeNanos = endTimeNanos,
                    Value = new List<Value> { new Value { FpVal = caloriesBurned } }
                };

                var caloriesDataSet = new Dataset
                {
                    DataSourceId = caloriesDataSourceId,
                    Point = new List<DataPoint> { caloriesDataPoint },
                    MinStartTimeNs = startTimeNanos,
                    MaxEndTimeNs = endTimeNanos
                };

                var caloriesRequest = _fitnessService.Users.DataSources.Datasets.Patch(caloriesDataSet, "me", caloriesDataSourceId, datasetId);
                await caloriesRequest.ExecuteAsync();

                _logger.LogInformation("Dados de calorias inseridos com sucesso.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inserir dados de calorias no Google Fit.");
                return false;
            }
        }
        public async Task<bool> InsertHydrationDataAsync(double hydrationLiters)
        {
            if (_fitnessService == null)
            {
                _logger.LogWarning("Google Fit Service não foi inicializado.");
                return false;
            }

            try
            {
                var endTime = DateTime.UtcNow;
                var startTime = endTime.AddMinutes(-1);
                var hydrationDataSourceId = "raw:com.google.hydration:1040818615650:phone:samsung:1040818615650";

                await CreateHydrationDataSourceAsync();

                var startTimeNanos = DateUtils.ToUnixTimestampInNanoseconds(startTime);
                var endTimeNanos = DateUtils.ToUnixTimestampInNanoseconds(endTime);
                string datasetId = $"{startTimeNanos}-{endTimeNanos}";

                var hydrationDataPoint = new DataPoint
                {
                    DataTypeName = "com.google.hydration",
                    StartTimeNanos = startTimeNanos,
                    EndTimeNanos = endTimeNanos,
                    Value = new List<Value> { new Value { FpVal = hydrationLiters } }
                };

                var hydrationDataSet = new Dataset
                {
                    DataSourceId = hydrationDataSourceId,
                    Point = new List<DataPoint> { hydrationDataPoint },
                    MinStartTimeNs = startTimeNanos,
                    MaxEndTimeNs = endTimeNanos
                };

                var hydrationRequest = _fitnessService.Users.DataSources.Datasets.Patch(hydrationDataSet, "me", hydrationDataSourceId, datasetId);
                await hydrationRequest.ExecuteAsync();

                _logger.LogInformation("Dados de hidratação inseridos com sucesso.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inserir dados de hidratação no Google Fit.");
                return false;
            }
        }


        public async Task<UserProfile> GetUserProfileAsync()
        {
            if (_fitnessService == null)
            {
                _logger.LogWarning("Google Fit Service não foi inicializado.");
                return null;
            }

            try
            {
                var request = _fitnessService.Users.DataSources.List("me");
                var response = await request.ExecuteAsync();

                _logger.LogInformation("Dados do usuário obtidos do Google Fit.");
                return await _userProfileMapper.Map(response, _fitnessService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter dados do Google Fit.");
                return null;
            }
        }

        public async Task<HealthData> GetHealthDataAsync()
        {
            if (_fitnessService == null)
            {
                _logger.LogWarning("Google Fit Service não foi inicializado.");
                return null;
            }

            try
            {
                var request = _fitnessService.Users.DataSources.List("me");
                var response = await request.ExecuteAsync();

                if (response == null || response.DataSource == null || !response.DataSource.Any())
                {
                    _logger.LogWarning("Nenhum dado retornado do Google Fit.");
                    return null;
                }

                return await Map(response, _fitnessService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter dados do Google Fit.");
                return null;
            }
        }
        public async Task<HealthData> Map(ListDataSourcesResponse response, FitnessService fitnessService)
        {
            if (response == null)
            {
                _logger.LogWarning("Resposta do Google Fit veio nula.");
                return null;
            }



            var healthData = new HealthData();

            var dataSources = response.DataSource?.Where(ds => ds.DataType.Name.Contains("com.google"));

            if (dataSources == null || !dataSources.Any())
            {
                _logger.LogWarning("Nenhuma fonte de dados relevante encontrada no Google Fit.");
                return null;
            }

            foreach (var source in dataSources)
            {
                switch (source.DataType.Name)
                {
                    case "com.google.nutrition":
                        var nutritionDataSource = response.DataSource?.FirstOrDefault(ds => ds.DataType.Name.Contains("com.google.nutrition"));
                        if (nutritionDataSource == null)
                        {
                            Console.WriteLine("Fonte de dados de altura não encontrada.");
                            return null;
                        }

                        long startOfWeek, endOfWeek;
                        (startOfWeek, endOfWeek) = DateUtils.GetWeekTimeRange();
                        string datasetId = $"{startOfWeek}-{endOfWeek}";

                        var dataPointsNutritionRequest = fitnessService.Users.DataSources.Datasets.Get("me", nutritionDataSource.DataStreamId, datasetId);
                        var dataPointsNutritionResponse = await dataPointsNutritionRequest.ExecuteAsync();

                        healthData.NutritionEntries = NutritionMapper.MapNutritionData(dataPointsNutritionResponse);

                        if (dataPointsNutritionResponse?.Point.FirstOrDefault().Value != null && dataPointsNutritionResponse.Point.Any())
                        {
                            foreach (var dataSet in dataPointsNutritionResponse.Point)
                            {
                                Console.WriteLine($"DataSet de nutrição: {dataSet}");
                                // Processar dados de nutrição aqui
                            }
                        }
                        else
                        {
                            Console.WriteLine("Nenhum dado retornado para nutrição.");
                        }

                        break;
                    case "com.google.hydration":
                        var hydrationDataSource = response.DataSource?.FirstOrDefault(ds => ds.DataType.Name.Contains("com.google.hydration"));
                        if (hydrationDataSource == null)
                        {
                            Console.WriteLine("Fonte de dados de altura não encontrada.");
                            return null;
                        }
                        long startOfWeek2, endOfWeek2;
                        (startOfWeek2, endOfWeek2) = DateUtils.GetWeekTimeRange();
                        string datasetId2 = $"{startOfWeek2}-{endOfWeek2}";

                        var dataPointsHydrationRequest = fitnessService.Users.DataSources.Datasets.Get("me", hydrationDataSource.DataStreamId, datasetId2);
                        var dataPointsHydrationResponse = await dataPointsHydrationRequest.ExecuteAsync();

                        healthData.HydrationEntries = HydrationMapper.MapHydrationData(dataPointsHydrationResponse);
                        foreach (var entry in healthData.HydrationEntries)
                        {
                            Console.WriteLine($"Data: {entry.Timestamp}, Hidratação: {entry.VolumeInLiters} L");
                        }

                        if (dataPointsHydrationResponse?.Point != null && dataPointsHydrationResponse.Point.Any())
                        {
                            foreach (var dataSet in dataPointsHydrationResponse.Point)
                            {
                                Console.WriteLine($"DataSet de nutrição: {dataSet}");
                                // Processar dados de nutrição aqui
                            }
                        }
                        else
                        {
                            Console.WriteLine("Nenhum dado retornado para Hidratação.");
                        }
                        break;

                }
            }

            _logger.LogInformation("Perfil do usuário mapeado com sucesso.");
            return healthData;
        }
    }
}
