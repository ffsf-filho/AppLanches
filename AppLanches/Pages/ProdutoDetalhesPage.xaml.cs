using AppLanches.Models;
using AppLanches.Services;
using AppLanches.Validations;
using System.Runtime.CompilerServices;

namespace AppLanches.Pages;

public partial class ProdutoDetalhesPage : ContentPage
{
	private readonly ApiService _apiService;
	private readonly IValidator _validator;
	private readonly int _produtoId;
	private bool _loginPageDisplayed = false;

	public ProdutoDetalhesPage(int produtoId, string produtoNome, ApiService apiService, IValidator validator)
	{
		InitializeComponent();
		_produtoId = produtoId;
		_apiService = apiService;
		_validator = validator;
		Title = produtoNome ?? "DEtalhe do Produto";
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await GetProdutoDetalhes(_produtoId);
	}

	private async Task<Produto?> GetProdutoDetalhes(int produtoId)
	{
		var(produtoDetalhe, erroMessage) = await _apiService.GetProdutoDetalhe(produtoId);

		if(erroMessage == "Unauthorized" && !_loginPageDisplayed)
		{
			await DisplayLoginPage();
			return null;
		}

		if(produtoDetalhe is null)
		{
			await DisplayAlert("Erro", erroMessage ?? "Não foi possível obter o produto.", "OK");
			return null;
		}

		ImagemProduto.Source = produtoDetalhe.CaminhoImagem;
		LblProdutoNome.Text = produtoDetalhe.Nome;
		LblProdutoPreco.Text = produtoDetalhe.Preco.ToString();
		LblProdutoDescricao.Text = produtoDetalhe.Detalhe;
		LblPrecoTotal.Text = produtoDetalhe.Preco.ToString();

		return produtoDetalhe;
	}

	private void ImagemBtnFavorito_Clicked(object sender, EventArgs e)
	{

	}

	private void BtnRemove_Clicked(object sender, EventArgs e)
	{
		if (int.TryParse(LblQuantidade.Text, out int quantidade) && decimal.TryParse(LblProdutoPreco.Text, out decimal precoUnitario))
		{
			//Decrementa a quantidade, e não permite que seja menor que 1
            quantidade = Math.Max(1, quantidade - 1);
            LblQuantidade.Text = quantidade.ToString();

            //Calcula o preço total
            //LblPrecoTotal.Text = (precoUnitario * quantidade).ToString("F2");//Formate como moeda
            decimal precoTtoal = quantidade * precoUnitario;
            LblPrecoTotal.Text = precoTtoal.ToString();
        }
		else
		{
            //Tratar caso as convesões falhem
            DisplayAlert("Erro", "Valores inválidos", "OK");
        }
	}

	private void BtnAdiciona_Clicked(object sender, EventArgs e)
	{
		if(int.TryParse(LblQuantidade.Text, out int quantidade) && decimal.TryParse(LblProdutoPreco.Text, out decimal precoUnitario))
		{
			//Incrementa a quantidade
			quantidade ++;
			LblQuantidade.Text = quantidade.ToString();

			//Calcula o preço total
			//LblPrecoTotal.Text = (precoUnitario * quantidade).ToString("F2");//Formate como moeda
			decimal precoTtoal = quantidade * precoUnitario;
			LblPrecoTotal.Text = precoTtoal.ToString();
		}
		else
		{
			//Tratar caso as convesões falhem
			DisplayAlert("Erro", "Valores inválidos", "OK");
		}
	}

	private async void BtnIncluirNocarrinho_Clicked(object sender, EventArgs e)
	{
		try
		{
			CarrinhoCompra carrinhoCompra = new()
			{
				Quantidade = Convert.ToInt32(LblQuantidade.Text),
				PrecoUnitario = Convert.ToDecimal(LblProdutoPreco.Text),
				ValorTotal = Convert.ToDecimal(LblPrecoTotal.Text),
				ProdutoId = _produtoId,
				ClienteId = Preferences.Get("usuarioid",0)
			};

			var response = await _apiService.AdicionaItemNoCarrinho(carrinhoCompra);

			if (response.Data)
			{
				await DisplayAlert("Sucesso", "Item adicionado ao carrinho !", "OK");
				await Navigation.PopAsync();
			}
			else
			{
				await DisplayAlert("Erro", $"Falha ao adicionar item: {response.ErrorMessage}", "OK");
			}
		}
		catch (Exception ex)
		{
            await DisplayAlert("Erro", $"Ocorreu um erro: {ex.Message}", "OK");
        }
	}

	private async Task DisplayLoginPage()
	{
		_loginPageDisplayed = true;
		await Navigation.PushAsync(new LoginPage(_apiService, _validator));
	}
}