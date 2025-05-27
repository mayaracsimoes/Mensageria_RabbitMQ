using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mensageria_Trabalho04
{

    public class ConsumidorInscritos
    {
        private readonly RabbitMQConnection _rabbitConnection;
        private readonly ServicoNotificacao _servicoNotificacao;
        private readonly String _genero;

        public ConsumidorInscritos(RabbitMQConnection rabbitConnection, ServicoNotificacao servicoNotificacao, string genero)
        {
            _rabbitConnection = rabbitConnection;
            _servicoNotificacao = servicoNotificacao;
            _genero = genero.ToLower();
        }

        public void Iniciar()
        {
            var canal = _rabbitConnection.CriarCanal();

            // Configuração da exchange e fila (mantida igual)
            canal.ExchangeDeclareAsync("eventos.musicais.topic", ExchangeType.Topic, durable: true);
            var nomeFila = canal.QueueDeclareAsync().Result.QueueName;
            canal.QueueBindAsync(
                        queue: nomeFila,
                        exchange: "eventos.musicais.topic",
                        routingKey: $"evento.{_genero}"
             );

            // Alteração crucial: Usar EventingBasicConsumer para handlers síncronos
            var consumidor = new AsyncEventingBasicConsumer(canal); // ← Tipo correto para sincronia

            consumidor.ReceivedAsync += (model, ea) =>
            {
                try
                {
                    var corpo = ea.Body.ToArray();
                    var mensagem = Encoding.UTF8.GetString(corpo);
                    var evento = JsonConvert.DeserializeObject<EventoMusical>(mensagem);

                    Console.WriteLine($" [x] Novo evento recebido: {evento}");

                    _servicoNotificacao.ProcessarNotificacoes(evento);

                    canal.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" [x] Erro: {ex.Message}");
                    canal.BasicNackAsync(ea.DeliveryTag, false, true);
                }

                return Task.CompletedTask;
            };

            canal.BasicConsumeAsync(
                queue: nomeFila,
                autoAck: false,
                consumer: consumidor);

            Console.WriteLine($" [*] Consumidor aguardando eventos... {_genero}");
        }
    }
}
