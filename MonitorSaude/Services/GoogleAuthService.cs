using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MonitorSaude.Interfaces;
using System.Net.Http;

namespace MonitorSaude.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly ILogger<GoogleAuthService> _logger;

        private const string ClientId = "1040818615650-ao6ejskfbd7ps6j5fbo1fg0hfcmt15ld.apps.googleusercontent.com";
        private const string RedirectUri = "com.companyname.monitorsaude://";
        private const string AuthUri = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenUri = "https://oauth2.googleapis.com/token";
        private const string Scope = "https://www.googleapis.com/auth/fitness.activity.read " +
                                     "https://www.googleapis.com/auth/fitness.body.read " +
                                     "https://www.googleapis.com/auth/fitness.nutrition.read " +
                                     "https://www.googleapis.com/auth/fitness.activity.write " +
                                     "https://www.googleapis.com/auth/fitness.body.write " +
                                     "https://www.googleapis.com/auth/fitness.nutrition.write";
        public GoogleAuthService(ILogger<GoogleAuthService> logger)
        {
            _logger = logger;
        }

        public async Task<string?> AuthenticateAsync()
        {
            try
            {
                // Verifica se há um refresh token salvo
                var refreshToken = await SecureStorage.GetAsync("refresh_token");
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogInformation("[AUTH] Usando refresh token para obter novo access token.");
                    return await RefreshAccessToken(refreshToken);
                }

                // Se não houver refresh_token, faz autenticação padrão
                var authUrl = $"{AuthUri}?response_type=code" +
                              $"&redirect_uri={RedirectUri}" +
                              $"&client_id={ClientId}" +
                              $"&scope={Scope}" +
                              "&include_granted_scopes=true&state=state_parameter_passthrough_value";

                var result = await WebAuthenticator.AuthenticateAsync(new Uri(authUrl), new Uri(RedirectUri));

                if (result.Properties.TryGetValue("code", out string authCode))
                {
                    _logger.LogInformation("[AUTH] Código de autenticação recebido: {AuthCode}", authCode);
                    return await ExchangeCodeForToken(authCode);
                }

                _logger.LogWarning("[AUTH] Nenhum código de autenticação foi retornado.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na autenticação");
                return null;
            }
        }

        private async Task<string?> ExchangeCodeForToken(string authCode)
        {
            try
            {
                using var client = new HttpClient();

                var parameters = new Dictionary<string, string>
                {
                    { "code", authCode },
                    { "client_id", ClientId },
                    { "redirect_uri", RedirectUri },
                    { "grant_type", "authorization_code" }
                };

                var content = new FormUrlEncodedContent(parameters);
                _logger.LogInformation("[TOKEN] Enviando código para troca de token...");

                var response = await client.PostAsync(TokenUri, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = JsonConvert.DeserializeObject<GoogleTokenResponse>(responseBody);
                    _logger.LogInformation("[TOKEN] Token de acesso recebido.");

                    // Salva o refresh_token
                    if (!string.IsNullOrEmpty(tokenResponse?.RefreshToken))
                    {
                        await SecureStorage.SetAsync("refresh_token", tokenResponse.RefreshToken);
                    }

                    return tokenResponse?.AccessToken;
                }

                _logger.LogError("[TOKEN] Falha ao obter o token. Status Code: {StatusCode}", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao trocar código por token");
            }

            return null;
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            using var client = new HttpClient();
            var requestUrl = $"https://oauth2.googleapis.com/revoke?token={refreshToken}";
            var response = await client.PostAsync(requestUrl, null);

            return response.IsSuccessStatusCode;
        }
        private async Task<string?> RefreshAccessToken(string refreshToken)
        {
            try
            {
                using var client = new HttpClient();

                var parameters = new Dictionary<string, string>
                {
                    { "client_id", ClientId },
                    { "refresh_token", refreshToken },
                    { "grant_type", "refresh_token" }
                };

                var content = new FormUrlEncodedContent(parameters);
                _logger.LogInformation("[TOKEN] Renovando access token usando refresh token.");

                var response = await client.PostAsync(TokenUri, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = JsonConvert.DeserializeObject<GoogleTokenResponse>(responseBody);
                    _logger.LogInformation("[TOKEN] Novo access token recebido.");

                    return tokenResponse?.AccessToken;
                }

                _logger.LogError("[TOKEN] Falha ao renovar o token. Status Code: {StatusCode}", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao renovar access token");
            }

            return null;
        }
    }

    public class GoogleTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonProperty("scope")]
        public string Scope { get; set; } = string.Empty;

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}