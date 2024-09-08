using AppLanches.Services;
using AppLanches.Validations;

namespace AppLanches.Pages;

public partial class PedidoDetalhesPage : ContentPage
{
    private readonly int _pedidoId;
    private readonly decimal _precoTotal;
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    public bool _loginPageDisplayed = false;

    public PedidoDetalhesPage(int pedidoId, decimal precoTotal, ApiService apiService, IValidator validator)
    {
        InitializeComponent();
        _pedidoId = pedidoId;
        _precoTotal = precoTotal;
        _apiService = apiService;
        _validator = validator;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await GetPedidoDetalhe(_pedidoId);
    }

    private async Task GetPedidoDetalhe(int pedidoId)
    {
        try
        {
            //Exibe o indicador de carregamento
            loadIndicator.IsRunning = true;
            loadIndicator.IsVisible = true;

            var (pedidoDetalhes, errorMessage) = await _apiService.GetPedidoDetalhes(pedidoId);

            if (errorMessage == "unauthorized" && !_loginPageDisplayed)
            {
                await DisplayLoginPage();
                return;
            }

            if(pedidoDetalhes is null)
            {
                await DisplayAlert("Erro", errorMessage ?? "Não foi possível obter os detalhes do pedido.", "OK");
                return;
            }

            LblPrecoTotal.Text = $" R${_precoTotal}";
            CvPedidosDetalhes.ItemsSource = pedidoDetalhes;
        }
        catch (Exception)
        {
            await DisplayAlert("Erro", "Ocorreu um erro ao obter os detalhes. Tente novamente mais tarde.", "OK");
        }
        finally
        {
            //Oculta o indicador de carregamento
            loadIndicator.IsRunning = false;
            loadIndicator.IsVisible = false;
        }
    }

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;
        await Navigation.PushAsync(new LoginPage(_apiService, _validator));
    }
}