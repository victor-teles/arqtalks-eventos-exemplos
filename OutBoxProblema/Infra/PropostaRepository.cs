using System.Data;
using Dapper;
using Domain;

namespace Infra;

public class PropostaRepository(IDbConnection dbConnection)
{

    public async Task CriarAsync(Propoposta propoposta)
    {
        string sql = $"INSERT INTO proposta (id, valor) VALUES (@Id, @Valor)";

        await dbConnection.ExecuteAsync(sql, propoposta);
    }
}
