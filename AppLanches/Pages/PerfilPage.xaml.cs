using AppLanches.Services;
using AppLanches.Validations;

namespace AppLanches.Pages;

public partial class PerfilPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private bool _loginPageDisplayed = false;

    public PerfilPage(ApiService apiService, IValidator validator)
    {
        InitializeComponent();
        LblNomeUsuario.Text = Preferences.Get("usuarionome", string.Empty);
        _apiService = apiService;
        _validator = validator;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ImgBtnPerfil.Source = await GetImagemPerfil();
    }

    private async void ImgBtnPerfil_Clicked(object sender, EventArgs e)
    {
        try
        {
            var imagemArray = await SelecionarImagemAsync();

            if (imagemArray is null)
            {
                await DisplayAlert("Erro", "N�o foi poss�vel carregar a imagem.", "OK");
                return;
            }

            ImgBtnPerfil.Source = ImageSource.FromStream(() => new MemoryStream(imagemArray));

            var response = await _apiService.UploadImagemUsuario(imagemArray);

            if (response.Data)
            {
                await DisplayAlert("", "Imagem enviada com sucesso.", "OK");
            }
            else
            {
                await DisplayAlert("Erro", response.ErrorMessage ?? "Ocorreu um erro desconhecido.", "Cancela");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
        }
    }

    private void TapMeusPedidos_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void TapMinhaConta_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void TapPerguntasFrequentes_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void ImgBtnLogout_Clicked(object sender, EventArgs e)
    {

    }

    private async Task<string?> GetImagemPerfil()
    {
        string imagemPadrao = AppConfig.PerfilImagemPadr�o;
        var (response, errorMessage) = await _apiService.GetImagemPerfilUsuario();

        if(errorMessage is not null)
        {
            switch (errorMessage)
            {
                case "Unauthorized":
                    if (!_loginPageDisplayed)
                    {
                        await DisplayLoginPage();
                        return null;
                    }
                    break;
                default:
                    await DisplayAlert("Erro", errorMessage ?? "N�o foi possivel obter a imagem.", "OK");
                    return imagemPadrao;
            }
        }

        if(response?.UrlImagem is not null)
        {
            return response.CaminhoImagem;
        }

        return imagemPadrao;
    }

    private async Task<byte[]?> SelecionarImagemAsync()
    {
        try
        {
            var arquivo = await MediaPicker.PickPhotoAsync();

            if (arquivo is null) return null;

            using(var stream = await arquivo.OpenReadAsync())
            using(var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
        catch (FeatureNotSupportedException ex)
        {
            await DisplayAlert("Erro", $"A funcionalidade n�o � suportada no dispositivo: {ex.Message}", "OK");
        }
        catch (PermissionException ex)
        {
            await DisplayAlert("Erro", $"Permiss�es n�o concedidas para acessar a camera ou galeria: {ex.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao selecionar a imagem: {ex.Message}", "OK");
        }

        return null;
    }

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;
        await Navigation.PushAsync(new LoginPage(_apiService, _validator));
    }
}
