namespace Domain;

public class PropopostaImplantadaEvento(Propoposta propoposta)
{
    public Propoposta Propoposta { get; set; } = propoposta;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
