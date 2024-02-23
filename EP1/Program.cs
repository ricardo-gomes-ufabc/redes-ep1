using CommandLine;

namespace EP1;

internal class Program
{
    public class Options
    {
        [Option('s', "servidor", Required = true, Default = false, HelpText = "Opção para rodar o programa em modo servidor", SetName = "ModoServidor")]
        public bool ModoServidor { get; set; }

        [Option('c', "cliente", Required = true, Default = false, HelpText = "Opção para rodar o programa em modo cliente", SetName = "ModoCliente")]
        public bool ModoCliente { get; set; }
    }

    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
                      .WithParsed(RunOptions)
                      .WithNotParsed(HandleParseError);
    }
    static void RunOptions(Options opts)
    {
        //handle options
    }
    static void HandleParseError(IEnumerable<Error> errs)
    {
        //handle errors
    }
}

