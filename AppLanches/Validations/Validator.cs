using System.Text.RegularExpressions;

namespace AppLanches.Validations;

public class Validator : IValidator
{
	public string NomeErro { get; set; } = "";
	public string EmailErro { get; set; } = "";
	public string TelefoneErro { get; set; } = "";
	public string SenhaErro { get; set; } = "";

	private const string NomeVazioErroMsg = "Por Favor, informe o seu nome.";
	private const string NomeInvalidoErroMsg = "Favor, informe um nome válido.";
	private const string EmailVazioErroMsg = "Por Favor, informe um email.";
	private const string EmailInvalidoErroMsg = "Favor, informe um email válido.";
	private const string TelefoneVazioErroMsg = "Por Favor, informe um telefone.";
	private const string TelefoneInvalidoErroMsg = "Favor, informe um telefone válido.";
	private const string SenhaVazioErroMsg = "Por Favor, informe a senha.";
	private const string SenhaInvalidoErroMsg = "A senha deve conter pelo menos 8 caracteres, incluindo letras e números.";

	public Task<bool> Validar(string nome, string email, string telefone, string senha)
	{
		bool isNomeValido = ValidarNome(nome);
		bool isEmailValido = ValidarEmail(email);
		bool isTelefoneValido = ValidarTelefone(telefone);
		bool isSenhaValida = ValidarSenha(senha);

		return Task.FromResult(isNomeValido && isEmailValido && isTelefoneValido && isSenhaValida);
	}

	private bool ValidarNome(string nome)
	{
		if (string.IsNullOrWhiteSpace(nome))
		{
			NomeErro = NomeVazioErroMsg;
			return false;
		}

		if (nome.Length < 3)
		{
			NomeErro = NomeInvalidoErroMsg;
			return false;
		}

		NomeErro = "";
		return true;
	}

	private bool ValidarEmail(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
		{
			EmailErro = EmailVazioErroMsg;
			return false;
		}

		if (!Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
		{
			EmailErro = EmailInvalidoErroMsg;
			return false;
		}

		EmailErro = "";
		return true;
	}

	private bool ValidarTelefone(string telefone)
	{

		if (string.IsNullOrWhiteSpace(telefone))
		{
			TelefoneErro = TelefoneVazioErroMsg;
			return false;
		}

		if (telefone.Length < 12)
		{
			TelefoneErro = TelefoneInvalidoErroMsg;
			return false;
		}

		TelefoneErro = "";
		return true;
	}


	private bool ValidarSenha(string senha)
	{
		if (string.IsNullOrWhiteSpace(senha))
		{
			SenhaErro = SenhaVazioErroMsg;
			return false;
		}

		if (senha.Length < 8 || !Regex.IsMatch(senha, @"[a-zA-Z]") || !Regex.IsMatch(senha, @"\d"))
		{
			SenhaErro = SenhaInvalidoErroMsg;
			return false;
		}

		SenhaErro = "";
		return true;
	}
}
