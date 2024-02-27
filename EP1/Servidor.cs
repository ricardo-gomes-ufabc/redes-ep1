using System.Net;

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

            IPEndPoint pontoConexaoLocal = new IPEndPoint(IPAddress.Loopback, porta);
            IPEndPoint? pontoConexaoRemoto = new IPEndPoint(IPAddress.Any, port: 0);

            _canal = new Canal(pontoConexaoLocal: pontoConexaoLocal,
                               pontoConexaoRemoto: pontoConexaoRemoto,
                               modoServidor: true);

            _canal.ReceberMensagens();

            _canal.Fechar();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}
