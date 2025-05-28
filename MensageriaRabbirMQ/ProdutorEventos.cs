using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mensageria_Trabalho04
{
    public class ProdutorEventos
    {
        private readonly RabbitMQConnection _rabbitConnection;
        private const string ExchangeName = "eventos.musicais.topic";
        private IChannel channel;

        public ProdutorEventos(RabbitMQConnection rabbitConnection)
        {
            _rabbitConnection = rabbitConnection;
            channel = rabbitConnection.CriarCanal();
            channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true);
        }

        public async Task PublicarEventoAsync(EventoMusical evento)
        {
            var message = JsonConvert.SerializeObject(evento);
            var body = Encoding.UTF8.GetBytes(message);
            var routingKey = $"evento.{evento.GeneroMusical.ToLower()}";

            await Task.Run(async () =>
            {
                await channel.BasicPublishAsync(
                    exchange: ExchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    body: new ReadOnlyMemory<byte>(body),
                    basicProperties: new BasicProperties(),
                    cancellationToken: default
                );
            });

            Console.WriteLine($" [x] Evento publicado assincronamente: {evento.NomeArtista}");
        }
    }
}