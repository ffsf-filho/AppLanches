using AppLanches.Pages;
using AppLanches.Services;
using AppLanches.Validations;

namespace AppLanches;

public partial class AppShell : Shell
{
	private readonly ApiService _apiService;
	private readonly IValidator _validator;

	public AppShell(ApiService apiService, IValidator validator)
	{
		InitializeComponent();
		_apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
		_validator = validator;
		ConfigureShell();
	}

	private void ConfigureShell()
	{
		HomePage homePage = new HomePage(_apiService, _validator);
		CarrinhoPage carrinhoPage = new CarrinhoPage(_apiService, _validator);
		FavoritosPage favoriosPage = new FavoritosPage();
		PerfilPage perfilPage = new PerfilPage(_apiService, _validator);

		Items.Add(new TabBar 
		{ 
			Items = {
				new ShellContent{Title="Home", Icon="home_svg", Content = homePage},
				new ShellContent{Title="Carrinho", Icon="shopping_cart_svg", Content= carrinhoPage},
				new ShellContent{Title="Favoritos", Icon="", Content=favoriosPage},
				new ShellContent{Title="Perfil", Icon="", Content=perfilPage}
			}
		});
	}
}

