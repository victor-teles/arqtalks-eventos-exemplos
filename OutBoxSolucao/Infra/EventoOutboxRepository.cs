using System.Data;
using Dapper;
using Domain;

namespace Infra;

public class EventoOutboxRepository(IDbConnection dbConnection)
{

    public async Task CriarAsync(IDbTransaction transaction, EventoOutbox eventoOutbox)
    {
        string sql = $"INSERT INTO \"eventosOutbox\" (evento, status, mensagem) VALUES (@Evento, @Status, @Mensagem)";

        await dbConnection.ExecuteAsync(sql, eventoOutbox, transaction);
    }

    public async Task AtualizarAsync(IDbTransaction transaction, EventoOutbox eventoOutbox)
    {
        string sql = $"UPDATE \"eventosOutbox\" set evento = @Evento, status = @Status, mensagem = @Mensagem where id = @Id";

        await dbConnection.ExecuteAsync(sql, eventoOutbox, transaction);
    }

    public async Task<IEnumerable<EventoOutbox>> ListarEventos(string status)
    {
        string sql = $"SELECT id,evento, status, mensagem from \"eventosOutbox\" where status = @status";
        return await dbConnection.QueryAsync<EventoOutbox>(sql, new { status });
    }
}
