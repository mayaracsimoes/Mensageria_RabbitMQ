using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Mensageria_Trabalho04
{

    public class ConsumidorInscritos
    {
        private readonly ServicoNotificacao _servicoNotificacao;
        private readonly String _genero;
        private IModel canal;

        public ConsumidorInscritos(RabbitMQConnection rabbitConnection, ServicoNotificacao servicoNotificacao, string genero)
        {
            canal = rabbitConnection.CriarCanal();
            _servicoNotificacao = servicoNotificacao;
            _genero = genero.ToLower();
        }

        public void Iniciar()
        {
            // Configuração da exchange e fila (mantida igual)
            canal.ExchangeDeclare("eventos.musicais.topic", ExchangeType.Topic, durable: true);
            var nomeFila = canal.QueueDeclare().QueueName;
            canal.QueueBind(
                        queue: nomeFila,
                        exchange: "eventos.musicais.topic",
                        routingKey: $"evento.{_genero}"
             );

            // Alteração crucial: Usar EventingBasicConsumer para handlers síncronos
            var consumidor = new AsyncEventingBasicConsumer(canal); // ← Tipo correto para sincronia

            consumidor.Received += (model, ea) =>
            {
                try
                {
                    var corpo = ea.Body.ToArray();
                    var mensagem = Encoding.UTF8.GetString(corpo);
                    var evento = JsonConvert.DeserializeObject<EventoMusical>(mensagem);

                    Console.WriteLine($" [x] Novo evento recebido: {evento}");

                    _servicoNotificacao.ProcessarNotificacoes(evento);

                    canal.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" [x] Erro: {ex.Message}");
                    canal.BasicNack(ea.DeliveryTag, false, true);
                }

                return Task.CompletedTask;
            };

            canal.BasicConsume(
                queue: nomeFila,
                autoAck: false,
                consumer: consumidor);

            Console.WriteLine($" [*] Consumidor aguardando eventos... {_genero}");
        }
    }
}
