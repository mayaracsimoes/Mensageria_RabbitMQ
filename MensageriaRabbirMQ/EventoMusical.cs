namespace Mensageria_Trabalho04
{
    public class EventoMusical
    {
        public string Id { get; set; }
        public string NomeArtista { get; set; }
        public string GeneroMusical { get; set; }
        public string Local { get; set; }
        public DateTime Data { get; set; }
        public string TipoEvento { get; set; } // SHOW ou FESTIVAL

        public override string ToString()
        {
            return $"{NomeArtista} - {GeneroMusical} em {Local} ({Data.ToShortDateString()})";
        }
    }
}
