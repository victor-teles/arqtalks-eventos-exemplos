namespace Domain;

public class Propoposta(long id)
{
    public long Id { get; set; } = id;
    public decimal Valor { get; set; }

    public void AlgumaRegraDeNegocio()
    {
        var random = new Random();
        Valor = random.Next(1, 1000);
    }
}
