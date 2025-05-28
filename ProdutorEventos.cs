using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Mensageria_Trabalho04
{
    public class ProdutorEventos
    {
        private const string ExchangeName = "eventos.musicais.topic";
        private IModel channel;

        public ProdutorEventos(RabbitMQConnection rabbitConnection)
        {
            channel = rabbitConnection.CreateChannelAsync().Result;
        }

        public async Task PublicarEventoAsync(EventoMusical evento)
        {
            await Task.Run(() =>
            {
                channel.ExchangeDeclare(
                    exchange: ExchangeName,
                    type: ExchangeType.Direct,
                    durable: true);
            });

            var message = JsonConvert.SerializeObject(evento);
            var body = Encoding.UTF8.GetBytes(message);

            var routingKey = $"evento.{evento.GeneroMusical.ToLower()}";

            await Task.Run(() =>
            {
                channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: routingKey,
                    basicProperties: null,
                    body: body);
            });

            Console.WriteLine($" [x] Evento publicado assincronamente: {evento.NomeArtista}");
        }
    }
}