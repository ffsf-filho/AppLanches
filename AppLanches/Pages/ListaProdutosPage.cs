using AppLanches.Services;
using AppLanches.Validations;

namespace AppLanches.Pages;

public class ListaProdutosPage : ContentView
{
	private readonly ApiService _apiService;
	private readonly IValidator _validator;

	public ListaProdutosPage(int id, string nome, ApiService apiService, IValidator validator)
	{
		Content = new VerticalStackLayout
		{
			Children = {
				new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Welcome to .NET MAUI!"
				}
			}
		};
		_apiService = apiService;
		_validator = validator;
	}
}