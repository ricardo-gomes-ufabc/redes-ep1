using System.Text.Json;

namespace EP1;

internal class Canal
{
    public uint PorcentagemTaxaEliminacao { get; set; }
    public uint DelayMilissegundos { get; set; }
    public uint PorcentagemTaxaDuplicacao { get; set; }
    public uint PorcentagemTaxaCorrupcao { get; set; }
    public uint TamanhoMaximoBytes { get; set; }

    public Canal()
    {
        CarregarConfigs();
        RodarCliente();
    }

    private void CarregarConfigs()
    {
        string json = File.ReadAllText(path: $@"{AppContext.BaseDirectory}/appsettings.json");

        using (JsonDocument document = JsonDocument.Parse(json))
        {
            JsonElement root = document.RootElement;

            uint porcentagemTaxaEliminacao = root.GetProperty("PorcentagemTaxaEliminacao").GetUInt32();
            uint delayMilissegundos = root.GetProperty("DelayMilissegundos").GetUInt32();
            uint porcentagemTaxaDuplicacao = root.GetProperty("PorcentagemTaxaDuplicacao").GetUInt32();
            uint porcentagemTaxaCorrupcao = root.GetProperty("PorcentagemTaxaCorrupcao").GetUInt32();
            uint tamanhoMaximoBytes = root.GetProperty("TamanhoMaximoBytes").GetUInt32();

            PorcentagemTaxaEliminacao = porcentagemTaxaEliminacao;
            DelayMilissegundos = delayMilissegundos;
            PorcentagemTaxaDuplicacao = porcentagemTaxaDuplicacao;
            PorcentagemTaxaCorrupcao = porcentagemTaxaCorrupcao;
            TamanhoMaximoBytes = tamanhoMaximoBytes;
        }
    }

    private void RodarCliente()
    {

    }
}

