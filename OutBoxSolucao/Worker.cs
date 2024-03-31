using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Coravel.Invocable;
using Infra;
using Npgsql;

public class Worker : IInvocable
{
    public int ExecutionCount = 0;

    public async Task Invoke()
    {
        await using var db = new NpgsqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTIONSTRING"));
        await db.OpenAsync();
        await using var serviceBus = new ServiceBusClient(Environment.GetEnvironmentVariable("SERVICE_BUS"));
        ServiceBusSender serviceBusSender = serviceBus.CreateSender("proposta_implantada");
        var eventoOutboxRepository = new EventoOutboxRepository(db);

        var eventosPendentes = await eventoOutboxRepository.ListarEventos("pendente");

        Stopwatch stopWatch = new();
        stopWatch.Start();

        foreach (var eventoOutbox in eventosPendentes)
        {
            var vaiFalharDurante5Segundos = stopWatch.Elapsed.TotalMilliseconds < 5000 && ExecutionCount == 0;
            using var transaction = await db.BeginTransactionAsync();

            try
            {
                if (vaiFalharDurante5Segundos)
                {
                    throw new Exception("ServiceBus indisponível");
                }

                await PublicarEvento(serviceBusSender, eventoOutbox.Mensagem);
                eventoOutbox.ConcluirEvento();
                await eventoOutboxRepository.AtualizarAsync(transaction, eventoOutbox);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                //Marcar evento com falha ou seguir como "pendente"
                continue;
            }
        }

        ExecutionCount++;
    }
    static async Task PublicarEvento(ServiceBusSender serviceBusSender, string evento)
    {
        var mensagem = new ServiceBusMessage(evento);
        await serviceBusSender.SendMessageAsync(mensagem);
    }
}
