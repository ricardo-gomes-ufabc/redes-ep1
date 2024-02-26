using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace EP1;

internal class Canal : IDisposable
{
    private readonly Random _aleatorio = new Random();

    private IPEndPoint _pontoConexaoLocal;
    private IPEndPoint _pontoConexaoRemoto;

    private UdpClient emissor = new UdpClient();
    private UdpClient receptor = new UdpClient();

    #region Configs

    private int _probabilidadeEliminacao;
    private int _delayMilissegundos;
    private int _probabilidadeDuplicacao;
    private int _probabilidadeCorrupcao;
    private int _tamanhoMaximoBytes;

    #endregion

    #region Config Extra

    private bool _modoServidor;

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

    public Canal(IPEndPoint pontoConexaoLocal, IPEndPoint pontoConexaoRemoto, bool modoServidor) : this(pontoConexaoLocal, modoServidor)
    {
        _pontoConexaoRemoto = pontoConexaoRemoto;
    }

    public Canal(IPEndPoint pontoConexaoLocal, bool modoServidor)
    {
        CarregarConfigs();

        _pontoConexaoLocal = pontoConexaoLocal;
        _modoServidor = modoServidor;

        receptor.Client.ReceiveTimeout = 5000;
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
            int tamanhoMaximoBytes = root.GetProperty("TamanhoMaximoBytes").GetInt32();

            _probabilidadeEliminacao = porcentagemTaxaEliminacao;
            _delayMilissegundos = delayMilissegundos;
            _probabilidadeDuplicacao = porcentagemTaxaDuplicacao;
            _probabilidadeCorrupcao = porcentagemTaxaCorrupcao;
            _tamanhoMaximoBytes = tamanhoMaximoBytes;
        }
    }

    #region Criação UDP

    public byte[] GerarSegmentoUDP(int tamanho)
    {
        byte[] segmento = new byte[tamanho];

        _aleatorio.NextBytes(segmento);

        return segmento;
    }

    #endregion

    #region Envio e Recebimento

    public void EnviarSegmentos(int quantidade)
    {
        for (int i = 0; i < quantidade; i++)
        {
            EnviarSegmento(GerarSegmentoUDP(_aleatorio.Next(minValue: 1, maxValue: _tamanhoMaximoBytes * 2)));
        }
    }

    public void EnviarSegmento(byte[] segmento)
    {
        Console.WriteLine("Enviando segmento UDP.");

        emissor.Send(segmento, _pontoConexaoRemoto);

        Console.WriteLine("Segmento UDP enviado");
    }

    public void ReceberSegmentos()
    {
        bool continuar = true;

        while (continuar)
        {
            continuar = ReceberSegmento();
        }
    }

    public bool ReceberSegmento()
    {
        try
        {
            byte[] segmentoRecebido = receptor.Receive(ref _pontoConexaoLocal);

            Console.WriteLine("Segmento UDP recebido.");

            byte[] segmentoModificado = segmentoRecebido.ToArray();

            AplicarPropriedades(ref segmentoModificado);

            return true;

        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Erro de socket: {ex.Message}");
        }

        return false;
    }

    public void ResponderMensagem()
    {
        EnviarSegmento(GerarSegmentoUDP(_aleatorio.Next(minValue: 1, maxValue: _tamanhoMaximoBytes * 2)));
    }

    #endregion

    #region Aplicação de Propiedades

    private void AplicarPropriedades(ref byte[] segmento)
    {
        if (DeveriaAplicarPropriedade(_probabilidadeEliminacao))
        {
            _totalMensagensEliminadas++;
            return;
        }

        Task.Delay(_delayMilissegundos).Wait();
        _totalMensagensAtrasadas++;

        if (DeveriaAplicarPropriedade(_probabilidadeDuplicacao))
        {
            _totalMensagensDuplicadas++;
            _totalMensagensRecebidas++;

            if (_modoServidor)
            {
                ResponderMensagem();
            }
        }

        if (DeveriaAplicarPropriedade(_probabilidadeCorrupcao))
        {
            CorromperSegmento(ref segmento);
        }

        CortarSegmento(ref segmento);
    }

    private bool DeveriaAplicarPropriedade(int probabilidade)
    {
        return _aleatorio.Next(minValue: 0, maxValue: 101) <= probabilidade;
    }

    private void CorromperSegmento(ref byte[] segmento)
    {
        int indice = _aleatorio.Next(minValue: 0, maxValue: segmento.Length);

        segmento[indice] = (byte)(~segmento[indice]); ;
    }

    private void CortarSegmento(ref byte[] segmento)
    {
        if (segmento.Length > _tamanhoMaximoBytes)
        {
            Array.Resize(ref segmento, _tamanhoMaximoBytes);
        }
    }

    #endregion

    #region Finalização

    public void Dispose()
    {
        emissor.Close();
        receptor.Close();

        emissor.Dispose();
        receptor.Dispose();
    }

    public void ConsolidarResultados()
    {
        Console.WriteLine(value: $"/n/n" +
                                 $"Total de mensagens enviadas: {_totalMensagensEnviadas}" +
                                 $"Total de mensagens recebidas: {_totalMensagensRecebidas}" +
                                 $"Total de mensagens eliminadas: {_totalMensagensEliminadas}" +
                                 $"Total de mensagens atrasadas: {_totalMensagensAtrasadas}" +
                                 $"Total de mensagens duplicadas: {_totalMensagensDuplicadas}" +
                                 $"Total de mensagens corrompidas: {_totalMensagensCorrompidas}" +
                                 $"Total de mensagens cortadas: {_totalMensagensCortadas}");
    }

    #endregion


}

