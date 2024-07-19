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

        bool enderecoSlavo = Preferences.ContainsKey("endereco");
        
        if (enderecoSlavo)
        {
            string nome = Preferences.Get("nome", string.Empty);
            string endereco = Preferences.Get("endereco", string.Empty);
            string telefone = Preferences.Get("telefone", string.Empty);

            LblEndereco.Text = $"{nome}\n{endereco} \n{telefone}";
        }
        else
        {
            LblEndereco.Text = "Informe o seu endereço.";
        }
    }

    private async void BtnIncrementar_Clicked(object sender, EventArgs e)
    {
		if (sender is Button button && button.BindingContext is CarrinhoCompraItem itemCarrinho)
		{
			itemCarrinho.Quantidade++;
			AtualizaPrecoTotal();
			await _apiService.AtualizaQuantidadeItemCarrinho(itemCarrinho.ProdutoId, "aumentar");
		}
    }

    private async void BtnDecrementar_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is CarrinhoCompraItem itemCarrinho)
        {
			if(itemCarrinho.Quantidade == 1)
			{
				return;
			}

            itemCarrinho.Quantidade--;
            AtualizaPrecoTotal();
            await _apiService.AtualizaQuantidadeItemCarrinho(itemCarrinho.ProdutoId, "diminuir");
        }
    }

    private async void BtnDeletar_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is CarrinhoCompraItem itemCarrinho)
        {
            bool resposta = await DisplayAlert("Confirmação", "Tem certeza que deseja excluir este item do carrinho?", "Sim", "Não");

            if (resposta)
            {
                ItensCarrinhoCompra.Remove(itemCarrinho);
                AtualizaPrecoTotal();
                await _apiService.AtualizaQuantidadeItemCarrinho(itemCarrinho.ProdutoId, "deletar");
            }
        }
    }

    private void BtnEditaEndereco_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new EnderecoPage());
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