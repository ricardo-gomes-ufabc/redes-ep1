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

            int port = Convert.ToInt32(Console.ReadLine());

            IPEndPoint pontoConexao = new IPEndPoint(Dns.GetHostAddresses("localhost")[0], port);

            _canal = new Canal(pontoConexaoLocal: pontoConexao, 
                               modoServidor: true);

            _canal.ReceberSegmentos();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}
