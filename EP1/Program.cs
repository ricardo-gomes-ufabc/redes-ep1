using CommandLine;
using EP1.Model;

namespace EP1;

internal class Program
{
    public class Options
    {
        [Option('s', "servidor", Required = false, Default = false, HelpText = "Opção para rodar o programa em modo servidor", SetName = "ModoServidor")]
        public bool ModoServidor { get; set; }
    }

    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
                      .WithParsed(RunOptions)
                      .WithNotParsed(HandleParseError);
    }
    static void RunOptions(Options opts)
    {
        if (opts.ModoServidor) 
        {
            RodarModoServidor();
        }
        else
        {
            RodarModoCliente();
        }
    }

    static void HandleParseError(IEnumerable<Error> errs)
    {
        //handle errors
    }

    private static void RodarModoCliente()
    {
        Cliente cliente = new Cliente();
    }

    private static void RodarModoServidor()
    {
        Servidor servidor = new Servidor();


    }
}

