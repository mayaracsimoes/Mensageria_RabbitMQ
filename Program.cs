using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mensageria_Trabalho04
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Sistema de Notificação de Eventos Musicais");
            Console.WriteLine("----------------------------------------");

            // Configura a conexão com o RabbitMQ
            var conexaoRabbit = new RabbitMQConnection();
            {
                // Cria os serviços
                var servicoNotificacao = new ServicoNotificacao(conexaoRabbit);
                var consumidorInscritos = new ConsumidorInscritos(conexaoRabbit, servicoNotificacao);
                var produtorEventos = new ProdutorEventos(conexaoRabbit);

                // Inicia o consumidor em segundo plano
                var taskConsumidor = System.Threading.Tasks.Task.Run(() => consumidorInscritos.Iniciar());

                // Menu simples para testar
                while (true)
                {
                    Console.WriteLine("\n1 - Publicar evento de Rock");
                    Console.WriteLine("2 - Publicar evento de Pop");
                    Console.WriteLine("3 - Publicar evento de Sertanejo");
                    Console.WriteLine("0 - Sair");
                    Console.Write("Escolha: ");

                    var opcao = Console.ReadLine();

                    if (opcao == "0") break;

                    EventoMusical novoEvento = null;

                    switch (opcao)
                    {
                        case "1":
                            novoEvento = new EventoMusical
                            {
                                Id = Guid.NewGuid().ToString(),
                                NomeArtista = "Banda de Rock",
                                GeneroMusical = "Rock",
                                Local = "São Paulo, SP",
                                Data = DateTime.Now.AddDays(30),
                                TipoEvento = "SHOW"
                            };
                            break;

                        case "2":
                            novoEvento = new EventoMusical
                            {
                                Id = Guid.NewGuid().ToString(),
                                NomeArtista = "Cantor Pop",
                                GeneroMusical = "Pop",
                                Local = "Rio de Janeiro, RJ",
                                Data = DateTime.Now.AddDays(45),
                                TipoEvento = "FESTIVAL"
                            };
                            break;

                        case "3":
                            novoEvento = new EventoMusical
                            {
                                Id = Guid.NewGuid().ToString(),
                                NomeArtista = "Dupla Sertaneja",
                                GeneroMusical = "Sertanejo",
                                Local = "Goiânia, GO",
                                Data = DateTime.Now.AddDays(60),
                                TipoEvento = "SHOW"
                            };
                            break;
                    }

                    if (novoEvento != null)
                    {
                        produtorEventos.PublicarEventoAsync(novoEvento);
                    }
                }
            }

            Console.WriteLine("Sistema encerrado.");
        }
    }
}
