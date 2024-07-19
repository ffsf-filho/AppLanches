using Android.App.AppSearch;
using AppLanches.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Text.Json;

namespace AppLanches.Services;

public class ApiService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<ApiService> _logger;
	private readonly string _baseUrl = AppConfig.BaseUrl; //"https://50h81fdr-7066.brs.devtunnels.ms/";
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

    public async Task<ApiResponse<bool>> AdicionaItemNoCarrinho(CarrinhoCompra carrinhoCompra)
    {
        try
        {
            string json = JsonSerializer.Serialize(carrinhoCompra, _serializerOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await PostRequest("api/ItensCarrinhoCompra", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Erro ao enviar requisição HTTP: {response.StatusCode}");
                return new ApiResponse<bool>
                {
                    ErrorMessage = $"Erro ao enviar requisição HTTP: {response.StatusCode}"
                };
            }

            return new ApiResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao adicionar item ao carrinho: {ex.Message}");
            return new ApiResponse<bool>
            {
                ErrorMessage = ex.Message
            };
        }
    }

	public async Task<(List<CarrinhoCompraItem>? CarrinhoCompraItems, string? ErrorMessage)> GetItensCarrinhoCompra(int usuarioId)
	{
		string endpoint = $"api/ItensCarrinhoCompra/{usuarioId}";
		return await GetAsync<List<CarrinhoCompraItem>>(endpoint);
	}
	
	public async Task<(bool data, string? ErrorMessage)> AtualizaQuantidadeItemCarrinho(int produtoId, string acao)
	{
		try
		{
			StringContent content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
			HttpResponseMessage response = await PutRequest($"api/itensCarrinhoCompra?produtoId={produtoId}&acao={acao}", content);

			if (response.IsSuccessStatusCode)
			{
				return (true, null);
			}
			else
			{
				if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					string errorMessage = "Unauthorized";
					_logger.LogWarning(errorMessage);
					return (false, errorMessage);
				}

				string generalErrorMessage = $"Errro na requisição: {response.ReasonPhrase}";
				_logger.LogError(generalErrorMessage);
				return (false, generalErrorMessage);
			}
		}
		catch (HttpRequestException ex)
		{
			string errorMessage = $"Errro de requisição HTTP: {ex.Message}";
			_logger.LogError(ex, errorMessage);
			return (false, errorMessage);
		}
		catch (Exception ex)
		{
            string errorMessage = $"Errro inesperado: {ex.Message}";
            _logger.LogError(ex, errorMessage);
            return (false, errorMessage);
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

	public async Task<HttpResponseMessage> PutRequest(string uri, HttpContent content)
	{
		string enderecoUrl = AppConfig.BaseUrl + uri;

		try
		{
			AddAutorizationHeader();
			HttpResponseMessage result = await _httpClient.PutAsync(enderecoUrl, content);
			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError($"Erro ao enviar requisição PUT para {uri}: {ex.Message}");
			return new HttpResponseMessage(HttpStatusCode.BadRequest);
		}
	}

	public async Task<(List<Categoria>? Categorias, string? ErrorMessage)> GetCategorias()
	{
		return await GetAsync<List<Categoria>>("api/categorias");
	}

	public async Task<(List<Produto>? Produtos, string? ErrorMessage)> GetProdutos(string tipoProduto, string categoriaIs)
	{
		string endpoint = $"api/Produtos?tipoProduto={tipoProduto}&categoriaId={categoriaIs}";
		return await GetAsync<List<Produto>>(endpoint);
	}

	private async Task<(T? Data, string? ErrorMessage)> GetAsync<T>(string endpoint)
	{
		try
		{
			AddAutorizationHeader();
			var response = await _httpClient.GetAsync(AppConfig.BaseUrl + endpoint);

			if (response.IsSuccessStatusCode)
			{
				string responseString = await response.Content.ReadAsStringAsync();
				T data = JsonSerializer.Deserialize<T>(responseString, _serializerOptions)!;
				return (data ?? Activator.CreateInstance<T>(), null);
			}
			else
			{
				if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					string errorMessage = "Unauthorized";
					_logger.LogWarning(errorMessage);
					return (default, errorMessage);
				}

				string generalErrorMessage = $"Erro na requisição: {response.ReasonPhrase}";
				_logger.LogError(generalErrorMessage);
				return (default, generalErrorMessage);
			}
		}
		catch (HttpRequestException ex)
		{
			string errorMessage = $"Erro de requisição HTTP: {ex.Message}";
			_logger.LogError(ex, errorMessage);
			return (default, errorMessage);
		}
		catch (JsonException ex)
		{
			string errorMessage = $"Erro de desserialização JSON: {ex.Message}";
			_logger.LogError(ex, errorMessage);
			return (default, errorMessage);
		}
		catch (Exception ex)
		{
			string errorMessage = $"Erro inesperado: {ex.Message}";
			_logger.LogError(ex, errorMessage);
			return (default, errorMessage);
		}
	}

	public async Task<(Produto? ProdutoDetalhe, string? ErrorMessage)> GetProdutoDetalhe(int produtoId)
	{
		string endpoint = $"api/produtos/{produtoId}";
		return await GetAsync<Produto>(endpoint);
	}

	private void AddAutorizationHeader()
	{
		var token = Preferences.Get("accesstoken", string.Empty);

		if (!String.IsNullOrEmpty(token))
		{
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}
	}
}
