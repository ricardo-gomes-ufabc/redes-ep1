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

            IPEndPoint pontoConexao = new IPEndPoint(IPAddress.Any, porta);

            _canal = new Canal(pontoConexaoLocal: pontoConexao, 
                               modoServidor: true);

            CancellationTokenSource fonteTokenCancelamento = new CancellationTokenSource();

            try
            {
                Task.Run(() => ReceberMensagens(fonteTokenCancelamento));

                Task.Delay(-1).Wait(fonteTokenCancelamento.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Servidor encerrado.");
            }
            finally
            {
                _canal.Fechar();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private static void ReceberMensagens(CancellationTokenSource tokenCancelamento)
    {
        byte[]? bufferReceptor;

        double timeoutSegundos = 15;

        using (Timer? timer = new Timer(_ => tokenCancelamento.Cancel(), null, TimeSpan.FromSeconds(timeoutSegundos), TimeSpan.FromMilliseconds(-1)))
        {
            try
            {
                while (!tokenCancelamento.IsCancellationRequested)
                {
                    if (_canal != null && _canal._socket.Available == 0 )
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    timer.Change(TimeSpan.FromSeconds(timeoutSegundos), Timeout.InfiniteTimeSpan);

                    bufferReceptor = _canal?.ReceberMensagem();

                    if (_canal != null && _canal.ProcessarMensagem(bufferReceptor) )
                    {
                        Console.WriteLine($"Mensagem UDP recebida.");

                        _canal?.EnviarMensagem(_canal.GerarMensagemUdp());

                        Console.WriteLine("Mensagem UDP respondida");
                    }
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
}
