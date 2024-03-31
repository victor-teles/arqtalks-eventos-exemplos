using Domain;
using Azure.Messaging.ServiceBus;
using DotNetEnv;
using System.Text.Json;
using Npgsql;
using Infra;

// Load .env file to environment variables
Env.TraversePath().Load();

await using var serviceBus = new ServiceBusClient(Environment.GetEnvironmentVariable("SERVICE_BUS"));
ServiceBusSender serviceBusSender = serviceBus.CreateSender("proposta_implantada");
await using var db = new NpgsqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTIONSTRING"));
await db.OpenAsync();

var propostaRepository = new PropostaRepository(db);
var deveEnviar = 500;
var falhaACada = 10;

for (int i = 0; i < deveEnviar; i++)
{
    var deveFalhar = i % falhaACada == 0;
    using var transaction = db.BeginTransaction();

    try
    {
        var proposta = new Propoposta(i);
        proposta.AlgumaRegraDeNegocio();
        var propopostaImplantadaEvento = new PropopostaImplantadaEvento(proposta);

        await propostaRepository.CriarAsync(proposta);

        await transaction.CommitAsync();

        if (deveFalhar)
        {
            throw new Exception("Esse evento não vai chegar!");
        }

        await PublicarEvento(serviceBusSender, propopostaImplantadaEvento);
    }
    catch
    {
        continue;
    }
}

static async Task PublicarEvento<T>(ServiceBusSender serviceBusSender, T evento)
{
    var mensagem = new ServiceBusMessage(JsonSerializer.Serialize(evento));
    await serviceBusSender.SendMessageAsync(mensagem);
}
