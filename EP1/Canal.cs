﻿using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace EP1;

internal class Canal
{
    private const int _tamanhoMáximoUDP = 0x10000;

    private readonly Random _aleatorio = new Random();

    private readonly object _locker = new object();

    #region Socket

    private IPEndPoint _pontoConexaoLocal;
    private IPEndPoint _pontoConexaoRemoto;
    private readonly UdpClient _socket = new UdpClient();

    #endregion

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

    #region Consolidação

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

        _socket.Client.Bind(_pontoConexaoLocal);

        _socket.Client.ReceiveTimeout = 15000;
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

    public byte[] GerarSegmentoUDP()
    {
        byte[] segmento = new byte[_aleatorio.Next(minValue: 1, maxValue: _tamanhoMáximoUDP)];

        _aleatorio.NextBytes(segmento);

        return segmento;
    }

    #endregion

    #region Envio e Recebimento

    public void EnviarMensagens(int quantidade, bool modoParalelo)
    {
        if (modoParalelo)
        {
            List<Task> tarefas = new List<Task>();

            for (int i = 0; i < quantidade; i++)
            {
                Task tarefa = Task.Run(() =>
                {
                    EnviarMensagem(GerarSegmentoUDP());
                    ReceberMensagens();
                });

                tarefas.Add(tarefa);
            }

            Task.WaitAll(tarefas.ToArray());
        }
        else
        {
            for (int i = 0; i < quantidade; i++)
            {
                EnviarMensagem(GerarSegmentoUDP());

                ReceberMensagem();
            }
        }
    }

    private void EnviarMensagem(byte[] mensagem)
    {
        _socket.Send(mensagem, _pontoConexaoRemoto);

        lock (_locker)
        {
            _totalMensagensEnviadas++;
        }
        
        Console.WriteLine("Segmento UDP enviado");
    }

    public void ReceberMensagens()
    {
        bool continuar = true;

        while (continuar)
        {
            continuar = ReceberMensagem();
        }
    }

    private bool ReceberMensagem()
    {
        try
        {
            byte[] mensagemRecebida = _socket.Receive(ref _pontoConexaoRemoto);

            _totalMensagensRecebidas++;

            Console.WriteLine("Segmento UDP recebido.");

            byte[] mensagemModificada = mensagemRecebida.ToArray();

            AplicarPropriedades(ref mensagemModificada);

            ValidarSegmento(original: mensagemRecebida, modificado: mensagemModificada);

            if (_modoServidor)
            {
                EnviarMensagem(GerarSegmentoUDP());
                Console.WriteLine("Segmento UDP respondido");
            }

            return true;
        }
        catch (SocketException ex)
        {
            if (!_modoServidor)
            {
                Console.WriteLine($"Erro de socket: {ex.Message}");
            }

            if (ex.SocketErrorCode == SocketError.TimedOut)
            {
                return false;
            }
        }

        return true;
    }

    private void ValidarSegmento(byte[] original, byte[] modificado)
    {
        if (original.Length != modificado.Length)
        {
            _totalMensagensCortadas++;
        }
        else if(!original.SequenceEqual(modificado))
        {
            _totalMensagensCorrompidas++;
        }
    }

    #endregion

    #region Aplicação de Propiedades

    private void AplicarPropriedades(ref byte[] mensagem)
    {
        if (DeveriaAplicarPropriedade(_probabilidadeEliminacao))
        {
            _totalMensagensEliminadas++;
            _totalMensagensRecebidas--;
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
                EnviarMensagem(GerarSegmentoUDP());
            }
        }

        if (DeveriaAplicarPropriedade(_probabilidadeCorrupcao))
        {
            CorromperSegmento(ref mensagem);
        }

        CortarSegmento(ref mensagem);
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

    public void Fechar()
    {
        ConsolidarResultados();

        _socket.Close();

        _socket.Dispose();
    }

    private void ConsolidarResultados()
    {
        Console.WriteLine(value: $"\n" +
                                 $"\nTotal de mensagens enviadas: {_totalMensagensEnviadas}" +
                                 $"\nTotal de mensagens recebidas: {_totalMensagensRecebidas}" +
                                 $"\nTotal de mensagens eliminadas: {_totalMensagensEliminadas}" +
                                 $"\nTotal de mensagens atrasadas: {_totalMensagensAtrasadas}" +
                                 $"\nTotal de mensagens duplicadas: {_totalMensagensDuplicadas}" +
                                 $"\nTotal de mensagens corrompidas: {_totalMensagensCorrompidas}" +
                                 $"\nTotal de mensagens cortadas: {_totalMensagensCortadas}");
    }

    #endregion


}

