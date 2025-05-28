using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace Mensageria_Trabalho04
{

    public class ConsumidorInscritos
    {
        private readonly ServicoNotificacao _servicoNotificacao;
        private readonly String _genero;
        private IChannel channel;

        public ConsumidorInscritos(RabbitMQConnection rabbitConnection, ServicoNotificacao servicoNotificacao, string genero)
        {
            _servicoNotificacao = servicoNotificacao;
            _genero = genero.ToLower();
            channel = rabbitConnection.CriarCanal();
            channel.ExchangeDeclareAsync("eventos.musicais.topic", ExchangeType.Topic, durable: true);
        }

        public void Iniciar()
        {
            // Configuração da exchange e fila (mantida igual)
            var nomeFila = channel.QueueDeclareAsync().Result.QueueName;
            channel.QueueBindAsync(
                        queue: nomeFila,
                        exchange: "eventos.musicais.topic",
                        routingKey: $"evento.{_genero}"
             );

            // Alteração crucial: Usar EventingBasicConsumer para handlers síncronos
            var consumidor = new AsyncEventingBasicConsumer(channel); // ← Tipo correto para sincronia

            consumidor.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var corpo = ea.Body.ToArray();
                    var mensagem = Encoding.UTF8.GetString(corpo);
                    var evento = JsonConvert.DeserializeObject<EventoMusical>(mensagem);

                    Console.WriteLine($" [x] Novo evento recebido: {evento}");

                    _servicoNotificacao.ProcessarNotificacoes(evento);

                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" [x] Erro: {ex.Message}");
                    await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            channel.BasicConsumeAsync(
                queue: nomeFila,
                autoAck: false,
                consumer: consumidor);

            Console.WriteLine($" [*] Consumidor aguardando eventos... {_genero}");
        }
    }
}
