using AppLanches.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AppLanches.Services;

public class ApiService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<ApiService> _logger;
	private readonly string _baseUrl = "https://pnftb8n2-7066.brs.devtunnels.ms";
	JsonSerializerOptions _serializerOptions;

	public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
	{
		_httpClient = httpClient;
		_logger = logger;
		_serializerOptions = new JsonSerializerOptions()
		{
			PropertyNameCaseInsensitive = true
		};
	}

	public async Task<ApiResponse<bool>> RegistarUsuario(string nome, string email, string telefone, string password)
	{
		try
		{
			Register register = new Register()
			{
				Nome = nome,
				Email = email,
				Telefone = telefone,
				Senha = password
			};

			string json = JsonSerializer.Serialize(register, _serializerOptions);
			StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await PostRequest("api/Usuarios/Register", content);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError($"Erro ao enviar requisição HTTP: {response.StatusCode}");

				return new ApiResponse<bool>
				{
					ErrorMessage = $"Erro ao enviar requisição HTTP: {response.StatusCode}",
				};
			}

			return new ApiResponse<bool> { Data = true };
		}
		catch (Exception ex)
		{
			_logger.LogError($"Erro ao registrar usuário: {ex.Message}");
			return new ApiResponse<bool> { ErrorMessage = ex.Message };

		}
	}

	public async Task<ApiResponse<bool>> Login(string email, string password)
	{
		try
		{
			Login login = new Login()
			{
				Email = email,
				Senha = password
			};

			string json = JsonSerializer.Serialize(login, _serializerOptions);
			StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await PostRequest("api/Usuarios/Login", content);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError($"Erro ao enviar requisição HTTP: {response.StatusCode}");

				return new ApiResponse<bool>
				{
					ErrorMessage = $"Erro ao enviar requisição HTTP: {response.StatusCode}",
				};
			}

			var jsonResult = await response.Content.ReadAsStringAsync();
			Token result = JsonSerializer.Deserialize<Token>(jsonResult, _serializerOptions)!;

			Preferences.Set("accesstoken", result!.AccessToken);
			Preferences.Set("usuarioid", (int)result.UsuarioId!);
			Preferences.Set("usuarionome", result.UsuarioNome);

			return new ApiResponse<bool> { Data = true };
		}
		catch (Exception ex)
		{
			_logger.LogError($"Erro no Login: {ex.Message}");
			return new ApiResponse<bool> { ErrorMessage = ex.Message };
		}
	}

	public async Task<HttpResponseMessage> PostRequest(string uri, HttpContent httpContent)
	{
		string enderecoUrl = _baseUrl + uri;

		try
		{
			HttpResponseMessage result = await _httpClient.PostAsync(enderecoUrl, httpContent);
			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError($"Erro ao enviar requisição POST para {uri}: {ex.Message}");
			return new HttpResponseMessage(HttpStatusCode.BadRequest);
		}
	}
}
