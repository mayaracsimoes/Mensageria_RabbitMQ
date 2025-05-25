using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Mensageria_Trabalho04
{

    public class ConsumidorInscritos
    {
        private readonly RabbitMQConnection _rabbitConnection;
        private readonly ServicoNotificacao _servicoNotificacao;

        public ConsumidorInscritos(RabbitMQConnection rabbitConnection, ServicoNotificacao servicoNotificacao)
        {
            _rabbitConnection = rabbitConnection;
            _servicoNotificacao = servicoNotificacao;
        }

        public void Iniciar()
        {
            var canal = _rabbitConnection.CriarCanal();

            // Declara a exchange (caso não exista)
            canal.ExchangeDeclare("eventos.musicais", ExchangeType.Topic, durable: true);

            // Cria uma fila temporária exclusiva para este consumidor
            var nomeFila = canal.QueueDeclare().QueueName;

            // Associa a fila ao exchange para todos os eventos musicais
            canal.QueueBind(
                queue: nomeFila,
                exchange: "eventos.musicais",
                routingKey: "evento.#");

            var consumidor = new EventingBasicConsumer(canal);

            consumidor.Received += (model, ea) =>
            {
                try
                {
                    var corpo = ea.Body.ToArray();
                    var mensagem = Encoding.UTF8.GetString(corpo);
                    var evento = JsonConvert.DeserializeObject<EventoMusical>(mensagem);

                    Console.WriteLine($" [x] Novo evento recebido: {evento}");

                    // Processa as notificações
                    _servicoNotificacao.ProcessarNotificacoes(evento);

                    // Confirma o processamento
                    canal.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" [x] Erro ao processar: {ex.Message}");
                    // Rejeita a mensagem (pode ser reprocessada)
                    canal.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            canal.BasicConsume(
                queue: nomeFila,
                autoAck: false, // Confirmação manual
                consumer: consumidor);

            Console.WriteLine(" [*] Consumidor aguardando eventos...");
        }
    }
}
