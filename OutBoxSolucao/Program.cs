using Domain;
using DotNetEnv;
using System.Text.Json;
using Npgsql;
using Infra;
using Microsoft.Extensions.Hosting;
using Coravel;
using Microsoft.Extensions.DependencyInjection;

// Load .env file to environment variables
Env.TraversePath().Load();


await using var db = new NpgsqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTIONSTRING"));
await db.OpenAsync();

var propostaRepository = new PropostaRepository(db);
var eventoOutboxRepository = new EventoOutboxRepository(db);
var deveEnviar = 500;

for (int i = 0; i < deveEnviar; i++)
{
    using var transaction = db.BeginTransaction();

    var proposta = new Propoposta(i);
    proposta.AlgumaRegraDeNegocio();

    await propostaRepository.CriarAsync(transaction, proposta);

    var propopostaImplantadaEvento = new PropopostaImplantadaEvento(proposta);
    var eventoOutbox = new EventoOutbox(PropopostaImplantadaEvento.Nome, "pendente", JsonSerializer.Serialize(propopostaImplantadaEvento));
    await eventoOutboxRepository.CriarAsync(transaction, eventoOutbox);

    await transaction.CommitAsync();
}

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddScheduler();
        services.AddSingleton<Worker>();
    })
    .Build();

host.Services.UseScheduler(scheduler =>
{
    scheduler.Schedule<Worker>()
    .EveryMinute()
    .RunOnceAtStart()
    .PreventOverlapping(nameof(Worker));
});

await host.RunAsync();
