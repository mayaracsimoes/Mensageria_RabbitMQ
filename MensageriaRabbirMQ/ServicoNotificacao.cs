using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mensageria_Trabalho04
{
    public class ServicoNotificacao
    {
        private readonly RabbitMQConnection _rabbitConnection;

        public ServicoNotificacao(RabbitMQConnection rabbitConnection)
        {
            _rabbitConnection = rabbitConnection;
        }

        public void ProcessarNotificacoes(EventoMusical evento)
        {
            // Simulação: obtém usuários interessados no gênero
            var usuarios = ObterUsuariosPorGenero(evento.GeneroMusical);

            foreach (var usuario in usuarios)
            {
                EnviarNotificacao(usuario, evento);
            }
        }

        private List<Usuario> ObterUsuariosPorGenero(string genero)
        {
            // Simulação - na prática, buscaria de um banco de dados
            return new List<Usuario>
            {
                new Usuario { Id = 1, Nome = "Fã de " + genero, Email = $"fa1@{genero}.com" },
                new Usuario { Id = 2, Nome = "Outro fã de " + genero, Email = $"fa2@{genero}.com" }
            };
        }

        private void EnviarNotificacao(Usuario usuario, EventoMusical evento)
        {
            using (var canal = _rabbitConnection.CriarCanal())
            {
                var exchangeNome = $"evento.{evento.GeneroMusical.ToLower()}";

                // Cria um exchange do tipo 'fanout' (envia para todas as filas associadas)
                canal.ExchangeDeclareAsync(exchangeNome, ExchangeType.Fanout, durable: true);

                var notificacao = new
                {
                    Usuario = usuario,
                    Evento = evento,
                    DataEnvio = DateTime.Now
                };

                var mensagem = JsonConvert.SerializeObject(notificacao);
                var corpo = Encoding.UTF8.GetBytes(mensagem);

                canal.BasicPublishAsync(
                   exchange: exchangeNome,
                   routingKey: "",
                   mandatory: true,
                   body: new ReadOnlyMemory<byte>(corpo),
                   basicProperties: new BasicProperties(),
                   cancellationToken: default
               );

                Console.WriteLine($" [x] Notificação enviada para {usuario.Nome} sobre {evento.NomeArtista}");
            }
        }
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
    }
}
