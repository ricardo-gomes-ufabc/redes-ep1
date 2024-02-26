﻿using System.Net;

namespace EP1;

internal class Servidor
{
    private static Canal _canal;

    private static void Main()
    {
        try
        { 
            Console.Write("Digite a porta do Servidor: ");

            int porta = Convert.ToInt32(Console.ReadLine());

            IPEndPoint pontoConexao = new IPEndPoint(IPAddress.Loopback, porta);

            _canal = new Canal(pontoConexaoLocal: pontoConexao, 
                               modoServidor: true);

            _canal.ReceberSegmentos();

            _canal.Fechar();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}
