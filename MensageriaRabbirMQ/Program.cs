namespace Mensageria_Trabalho04
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Sistema de Notificação de Eventos Musicais");
            Console.WriteLine("----------------------------------------");

            var generos = new List<string> { "rock", "pop", "sertanejo" };

            // Configura a conexão com o RabbitMQ
            var conexaoRabbit = RabbitMQConnection.CreateAsync().Result;
            {
                // Cria o serviço
                var servicoNotificacao = new ServicoNotificacao(conexaoRabbit);

                // Cria consumidores específicos para cada gênero, 1 tópico e várias filas
                generos.ForEach(genero =>
                {
                    var consumidor = new ConsumidorInscritos(conexaoRabbit, servicoNotificacao, genero);
                    Task.Run(() => consumidor.Iniciar()); // Inicia cada consumidor em segundo plano
                });

                var produtorEventos = new ProdutorEventos(conexaoRabbit);

                // Menu interativo
                while (true)
                {
                    int index = 1;
                    generos.ForEach(genero =>
                    {
                        Console.WriteLine($"{index} - Publicar evento de {genero} (opção genérica)");
                        index++;
                    });
                    Console.WriteLine("0 - Sair");
                    Console.WriteLine("Escolha:");

                    var opcao = Console.ReadLine();

                    if (opcao == "0") break;

                    EventoMusical novoEvento = CriarEventoPorOpcao(opcao);

                    if (novoEvento != null)
                    {
                        await produtorEventos.PublicarEventoAsync(novoEvento);
                    }
                }

                Console.WriteLine("Finalizando consumidores...");
            }

            Console.WriteLine("Sistema encerrado.");
        }

        private static EventoMusical CriarEventoPorOpcao(string opcao)
        {
            switch (opcao)
            {
                case "1":
                    return new EventoMusical
                    {
                        Id = Guid.NewGuid().ToString(),
                        NomeArtista = "Banda de Rock",
                        GeneroMusical = "Rock",
                        Local = "São Paulo, SP",
                        Data = DateTime.Now.AddDays(30),
                        TipoEvento = "SHOW"
                    };

                case "2":
                    return new EventoMusical
                    {
                        Id = Guid.NewGuid().ToString(),
                        NomeArtista = "Cantor Pop",
                        GeneroMusical = "Pop",
                        Local = "Rio de Janeiro, RJ",
                        Data = DateTime.Now.AddDays(45),
                        TipoEvento = "FESTIVAL"
                    };

                case "3":
                    return new EventoMusical
                    {
                        Id = Guid.NewGuid().ToString(),
                        NomeArtista = "Dupla Sertaneja",
                        GeneroMusical = "Sertanejo",
                        Local = "Goiânia, GO",
                        Data = DateTime.Now.AddDays(60),
                        TipoEvento = "SHOW"
                    };

                default:
                    Console.WriteLine("Opção inválida!");
                    return null;
            }
        }

    }
}
