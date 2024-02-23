using System.Text.Json;

namespace EP1;

internal class Configs
{
    public double TaxaEliminacao { get; set; }
    public uint DelayMilissegundos { get; set; }
    public double TaxaDuplicacao { get; set; }
    public double TaxaCorrupcao { get; set; }
    public uint TamanhoMaximoBytes { get; set; }
}

internal class Canal
{
    public Configs Configs { get; private set; }

    public Canal()
    {
        CarregarConfigs();
    }

    private void CarregarConfigs()
    {
        string json = File.ReadAllText(path: $@"{AppContext.BaseDirectory}/appsettings.json");

        Configs = JsonSerializer.Deserialize<Configs>(json);
    }
}

