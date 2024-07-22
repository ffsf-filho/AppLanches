using AppLanches.Models;
using AppLanches.Services;
using AppLanches.Validations;

namespace AppLanches.Pages;

public partial class HomePage : ContentPage
{
	private readonly ApiService _apiService;
	private readonly IValidator _validator;
	private bool _loginPageDisplayed = false;
	private bool _isDataLoaded = false;

	public HomePage(ApiService apiService, IValidator validator)
	{
		InitializeComponent();
		LblNomeUsuario.Text = "Olá, " + Preferences.Get("usuarionome", string.Empty);
		_apiService = apiService ?? throw new ArgumentNullException(nameof(apiService)); ;
		_validator = validator;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (!_isDataLoaded)
		{
			await LoadDataAsync();
			_isDataLoaded = true;
		}
	}

	private async Task LoadDataAsync()
	{
		var categoriasTask = GetListaCategorias();
		var maisVendidosTask = GetMaisVendidos();
		var popularesTask = GetPopulares();

		await Task.WhenAll(categoriasTask, maisVendidosTask, popularesTask);
	}

	private async Task<IEnumerable<Categoria>> GetListaCategorias()
	{
		try
		{
			var (categorias, errorMessage) = await _apiService.GetCategorias();
			
			if(errorMessage == "Unauthorized" && !_loginPageDisplayed)
			{
				await DisplayLoginPage();
				return Enumerable.Empty<Categoria>();
			}

			if (categorias == null)
			{
				await DisplayAlert("Erro", errorMessage ?? "Não foi possível obter as categorias.", "OK");
				return Enumerable.Empty<Categoria>();
			}

			CvCategorias.ItemsSource = categorias;
			return categorias;
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
			return Enumerable.Empty<Categoria>();
		}
	}

	private async Task<IEnumerable<Produto>> GetMaisVendidos()
	{
		try
		{
			var (produtos, errorMessage) = await _apiService.GetProdutos("maisvendido", string.Empty);

			if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
			{
				await DisplayLoginPage();
				return Enumerable.Empty<Produto>();
			}

			if (produtos == null)
			{
				await DisplayAlert("Erro", errorMessage ?? "Não foi possível obter as categorias.", "OK");
				return Enumerable.Empty<Produto>();
			}

			CvMaisVendidos.ItemsSource = produtos;
			return produtos;
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
			return Enumerable.Empty<Produto>();
		}
	}

	private async Task<IEnumerable<Produto>> GetPopulares()
	{
		try
		{
			var (produtos, errorMessage) = await _apiService.GetProdutos("popular", string.Empty);

			if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
			{
				await DisplayLoginPage();
				return Enumerable.Empty<Produto>();
			}

			if (produtos == null)
			{
				await DisplayAlert("Erro", errorMessage ?? "Não foi possível obter as categorias.", "OK");
				return Enumerable.Empty<Produto>();
			}

			CvPopulares.ItemsSource = produtos;
			return produtos;
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
			return Enumerable.Empty<Produto>();
		}
	}

	private async Task DisplayLoginPage()
	{
		_loginPageDisplayed = true;
		await Navigation.PushAsync(new LoginPage(_apiService, _validator));
	}

	private void CvCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		Categoria? currentSelection = e.CurrentSelection.FirstOrDefault() as Categoria;

		if(currentSelection == null) { return;}

		Navigation.PushAsync(
			new ListaProdutosPage(currentSelection.Id, currentSelection.Nome!, _apiService, _validator)
			);
		((CollectionView) sender).SelectedItem = null;
    }

    private void CvMaisVendidos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
		if(sender is CollectionView collectionView)
		{
			NavigateTOProdutoDetalhesPage(collectionView, e);
		}
    }

    private void CvPopulares_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            NavigateTOProdutoDetalhesPage(collectionView, e);
        }
    }

    private void NavigateTOProdutoDetalhesPage(CollectionView collectionView, SelectionChangedEventArgs e)
    {
		Produto? currentSelection = e.CurrentSelection.FirstOrDefault() as Produto;

        if (currentSelection == null) { return; }

        Navigation.PushAsync(
            new ProdutoDetalhesPage(currentSelection.Id, currentSelection.Nome!, _apiService, _validator)
            );

		collectionView.SelectedItem = null;
    }
}