using System.Net;
using System.Net.Sockets;

namespace EP1;

internal class Servidor
{
    private static Canal? _canal;

    private static void Main()
    {
        try
        { 
            Console.Write("Digite a porta do Servidor: ");

            int porta = Convert.ToInt32(Console.ReadLine());

            IPEndPoint pontoConexao = new IPEndPoint(IPAddress.Loopback, porta);

            _canal = new Canal(pontoConexaoLocal: pontoConexao, 
                               modoServidor: true);

            ReceberMensagens();

            _canal.Fechar();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private static void ReceberMensagens()
    {
        byte[]? bufferReceptor;

        try
        {
            while (true)
            {
                bufferReceptor = _canal?.ReceberMensagem();

                Thread responder = new Thread(() =>
                {
                    if (_canal != null && _canal.ProcessarMensagem(bufferReceptor))
                    {
                        _canal?.EnviarMensagem(_canal.GerarMensagemUdp());
                        Console.WriteLine("Mensagem UDP respondida");
                    }
                });

                responder.Start();
            }
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode != SocketError.TimedOut)
            {
                throw;
            }
        }
    }
}
