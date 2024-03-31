namespace Domain;

public class EventoOutbox
{
    public EventoOutbox(string evento, string status, string mensagem)
    {
        Evento = evento;
        Status = status;
        Mensagem = mensagem;
    }

    public EventoOutbox()
    {
    }

    public int Id { get; set; }
    public string Evento { get; set; }
    public string Status { get; set; }
    public string Mensagem { get; set; }

    public void ConcluirEvento()
    {
        this.Status = "enviado";
    }
}
