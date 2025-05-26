using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

public class RabbitMQConnection
{
    private readonly IConnection _connection;

    private RabbitMQConnection(IConnection connection)
    {
        _connection = connection;
    }

    public static async Task<RabbitMQConnection> CreateAsync()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            DispatchConsumersAsync = true
        };

        var connection = factory.CreateConnection();
        return new RabbitMQConnection(connection);
    }

    public async Task<IModel> CreateChannelAsync()
    {
        return await Task.Run(() => _connection.CreateModel());
    }

    public IModel CriarCanal()
    {
        return _connection.CreateModel();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await Task.Run(() =>
            {
                _connection.Close();
                _connection.Dispose();
            });
        }
        Console.WriteLine("Conexão com RabbitMQ fechada assincronamente.");
    }
}