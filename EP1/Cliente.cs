using System.Net;

namespace EP1;

internal class Cliente
{
    private static Canal? _canal;

    private static void Main()
    {
        try
        {
            Console.Write("Digite a porta do Cliente: ");

            int portaCliente = Convert.ToInt32(Console.ReadLine());

            IPEndPoint pontoConexaoLocal = new IPEndPoint(IPAddress.Loopback, portaCliente);

            Console.Write("Digite o IP do Servidor: ");

            string? ipServidor = Console.ReadLine();

            Console.Write("Digite a porta do Servidor: ");

            int portaServidor = Convert.ToInt32(Console.ReadLine());

            IPEndPoint? pontoConexaoRemoto;

            pontoConexaoRemoto = string.IsNullOrEmpty(ipServidor) ? 
                                 new IPEndPoint(IPAddress.Loopback, portaServidor) : 
                                 new IPEndPoint(IPAddress.Parse(ipServidor), portaServidor);

            _canal = new Canal(pontoConexaoRemoto: pontoConexaoRemoto,
                               pontoConexaoLocal: pontoConexaoLocal,
                               modoServidor: false);

            Console.Write("Deseja enviar de forma paralela [S/N]?: ");

            string? paralelismo = Console.ReadLine();
            bool modoParalelo = !string.IsNullOrEmpty(paralelismo) && paralelismo.ToLower() == "s";

            Console.Write("Digite a quantidade de mensagens a serem enviadas: ");

            int quantidadeMensagens = Convert.ToInt32(Console.ReadLine());

            _canal.EnviarMensagens(quantidadeMensagens, modoParalelo);

            _canal.Fechar();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}

