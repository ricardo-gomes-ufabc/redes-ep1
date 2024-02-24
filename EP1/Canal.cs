using System.Text.Json;

namespace EP1;

internal class Canal
{
    private readonly Random _aleatorio = new Random();

    #region Configs

    private int _probabilidadeEliminacao;
    private int _delayMilissegundos;
    private int _probabilidadeDuplicacao;
    private int _probabilidadeCorrupcao;
    private int _tamanhoMaximoBytes;

    #endregion

    #region Config Extra

    private int _quantidadeMensagensParaEnviar;
    private int _probabilidadeDelay = 5;

    #endregion

    #region Consolidacao

    private uint _totalMensagensEnviadas;
    private uint _totalMensagensRecebidas;
    private uint _totalMensagensEliminadas;
    private uint _totalMensagensAtrasadas;
    private uint _totalMensagensDuplicadas;
    private uint _totalMensagensCorrompidas;
    private uint _totalMensagensCortadas;

    #endregion

    public Canal(int quantidadeMensagensParaEnviar)
    {
        CarregarConfigs();

        _quantidadeMensagensParaEnviar = quantidadeMensagensParaEnviar;
    }

    private void CarregarConfigs()
    {
        string json = File.ReadAllText(path: $@"{AppContext.BaseDirectory}/appsettings.json");

        using (JsonDocument document = JsonDocument.Parse(json))
        {
            JsonElement root = document.RootElement;

            int porcentagemTaxaEliminacao = root.GetProperty("ProbabilidadeEliminacao").GetInt32();
            int delayMilissegundos = root.GetProperty("DelayMilissegundos").GetInt32();
            int porcentagemTaxaDuplicacao = root.GetProperty("ProbabilidadeDuplicacao").GetInt32();
            int porcentagemTaxaCorrupcao = root.GetProperty("ProbabilidadeCorrupcao").GetInt32();
            int tamanhoMaximoBytes = root.GetProperty("TamanhoMaximoBytes").GeInt32();

            _probabilidadeEliminacao = porcentagemTaxaEliminacao;
            _delayMilissegundos = delayMilissegundos;
            _probabilidadeDuplicacao = porcentagemTaxaDuplicacao;
            _probabilidadeCorrupcao = porcentagemTaxaCorrupcao;
            _tamanhoMaximoBytes = tamanhoMaximoBytes;
        }
    }

    #region Envio e Recebimento

    public void EnviarMensagem(byte[] mensagem)
    {

    }

    public byte[]? ReceberMensagem()
    {
        byte[]? mensagem = null;



        return mensagem;
    }

    #endregion

    #region Aplicacao de Propiedades

    private byte[]? AplicarPropriedades(byte[] mensagem)
    {
        if (DeveriaAplicarPropriedade(_probabilidadeEliminacao))
        {
            return null;
        }

        if (DeveriaAplicarPropriedade(_probabilidadeDelay))
        {
            Task.Delay(_delayMilissegundos).Wait();
        }

        if (DeveriaAplicarPropriedade(_probabilidadeDuplicacao))
        {
            
        }

        if (DeveriaAplicarPropriedade(_probabilidadeCorrupcao))
        {
            mensagem = CorromperSegmento(mensagem);
        }

        mensagem = CortarSegmento(mensagem);

        return mensagem;
    }

    private bool DeveriaAplicarPropriedade(int probabilidade)
    {
        return _aleatorio.Next(minValue: 0, maxValue: 101) <= probabilidade;
    }

    private byte[] CorromperSegmento(byte[] mensagem)
    {
        int indice = _aleatorio.Next(minValue: 0, maxValue: mensagem.Length);
        mensagem[indice] = (byte)(~mensagem[indice]);
        return mensagem;
    }

    private byte[] CortarSegmento(byte[] mensagem)
    {
        if (mensagem.Length > _tamanhoMaximoBytes)
        {
            Array.Resize(ref mensagem, _tamanhoMaximoBytes);
        }

        return mensagem;
    }

    #endregion

    public byte[] GenerateRandomUdpMessage(int tamanho)
    {
        byte[] mensagem = new byte[tamanho];
        _aleatorio.NextBytes(mensagem);
        return mensagem;
    }
}

