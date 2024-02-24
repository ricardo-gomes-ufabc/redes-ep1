namespace EP1;

internal class Servidor
{
    private static Canal canal = new Canal();

    private static void Main()
    {
        try
        { 
            Console.Write("Digite a porta do Servidor: ");

            int port = Convert.ToInt32(Console.ReadLine());
        }
        catch (FormatException e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}
