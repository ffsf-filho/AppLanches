using AppLanches.Models;
using AppLanches.Services;
using AppLanches.Validations;
using System.Collections.ObjectModel;

namespace AppLanches.Pages;

public partial class CarrinhoPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private bool _loginPageDisplayed = false;
	private ObservableCollection<CarrinhoCompraItem> ItensCarrinhoCompra = new ObservableCollection<CarrinhoCompraItem>();

    public CarrinhoPage(ApiService apiService, IValidator validator)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
		await GetItensCarrinhoCompra();
    }

    private void BtnEditaEndereco_Clicked(object sender, EventArgs e)
    {

    }

	private async Task<IEnumerable<CarrinhoCompraItem>> GetItensCarrinhoCompra()
	{
		try
		{
			int usuarioId = Preferences.Get("usuarioid",0);
			var (itensCarrinhoCompra, errorMessage) = await _apiService.GetItensCarrinhoCompra(usuarioId);

			if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
			{
				await DisplayLoginPage();
				return Enumerable.Empty<CarrinhoCompraItem>();
			}

			if (itensCarrinhoCompra == null)
			{
                await DisplayAlert("Erro", errorMessage ?? "Não foi possivel obter os itens do carrinho", "OK");
                return Enumerable.Empty<CarrinhoCompraItem>();
            }

			ItensCarrinhoCompra.Clear();

			foreach (CarrinhoCompraItem item in itensCarrinhoCompra)
			{
				ItensCarrinhoCompra.Add(item);
			}

			CvCarrinho.ItemsSource = ItensCarrinhoCompra;
			AtualizaPrecoTotal();
			return itensCarrinhoCompra;
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
			return Enumerable.Empty<CarrinhoCompraItem>();
		}
	}

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;
        await Navigation.PushAsync(new LoginPage(_apiService, _validator));
    }

	private void AtualizaPrecoTotal() 
	{
		try
		{
			decimal precoTotal = ItensCarrinhoCompra.Sum(item => item.Preco * item.Quantidade);
			LblPrecoTotal.Text = precoTotal.ToString();
		}
		catch (Exception ex)
		{
            DisplayAlert("Erro", $"Ocorreu um erro ao atualizar o preço total: {ex.Message}", "OK");
        }
	}
}