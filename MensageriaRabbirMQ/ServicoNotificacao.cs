using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mensageria_Trabalho04
{
    public class ServicoNotificacao
    {
        private IChannel _channel;

        public ServicoNotificacao(RabbitMQConnection rabbitConnection)
        {
            _channel = rabbitConnection.CriarCanal();
            _channel.ExchangeDeclareAsync("notificacao.*", ExchangeType.Fanout, durable: true);
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
            var exchangeNome = $"notificacao.{evento.GeneroMusical.ToLower()}";

            // Cria um exchange do tipo 'fanout' (envia para todas as filas associadas)

            var notificacao = new
            {
                Usuario = usuario,
                Evento = evento,
                DataEnvio = DateTime.Now
            };

            var mensagem = JsonConvert.SerializeObject(notificacao);
            var corpo = Encoding.UTF8.GetBytes(mensagem);

            _channel.BasicPublishAsync(
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

    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
    }
}
