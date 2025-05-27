using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Mensageria_Trabalho04
{
    internal class Producer
    {

        private readonly RabbitMQConnection _rabbitMQConnection;
        private readonly ConnectionFactory _factory;

        public Producer(RabbitMQConnection rabbitMQConnection)
        {
            _rabbitMQConnection = rabbitMQConnection;

            _factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
        }

        public async Task PublicarMensagemAsync(string mensagem)
        {
            var channel = await _rabbitMQConnection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(mensagem);

            await channel.BasicPublishAsync(exchange: "eventos.musicais.topic", routingKey: "eventos", body: body);

            Console.WriteLine($" [x] Sent {mensagem}");
        }

    }
}
