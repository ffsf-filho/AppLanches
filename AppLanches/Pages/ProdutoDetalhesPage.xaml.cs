using AppLanches.Models;
using AppLanches.Services;
using AppLanches.Validations;

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
		int quantidade = Convert.ToInt32(LblQuantidade.Text);
		decimal preco = Convert.ToDecimal(LblProdutoPreco.Text);

		quantidade = quantidade > 0 ? quantidade - 1 : 0;
		LblQuantidade.Text = quantidade.ToString();
		LblPrecoTotal.Text = (preco * quantidade).ToString("F2");
	}

	private void BtnAdiciona_Clicked(object sender, EventArgs e)
	{
		int quantidade = Convert.ToInt32(LblQuantidade.Text);
		decimal preco = Convert.ToDecimal(LblProdutoPreco.Text);

		quantidade += 1;
		LblQuantidade.Text = quantidade.ToString();
		LblPrecoTotal.Text = (preco * quantidade).ToString("F2");
	}

	private void BtnIncluirNocarrinho_Clicked(object sender, EventArgs e)
	{

	}

	private async Task DisplayLoginPage()
	{
		_loginPageDisplayed = true;
		await Navigation.PushAsync(new LoginPage(_apiService, _validator));
	}
}