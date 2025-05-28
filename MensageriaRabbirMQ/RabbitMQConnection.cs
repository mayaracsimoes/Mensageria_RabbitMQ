using RabbitMQ.Client;

public class RabbitMQConnection
{
    private readonly IConnection _connection;
    private IChannel _sharedChannel;


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
            Password = "guest"
        };

        var connection = factory.CreateConnectionAsync();
        return new RabbitMQConnection(connection.Result);
    }

    public async Task<IChannel> CreateChannelAsync()
    {
        return await Task.Run(() => _connection.CreateChannelAsync());
    }

    public IChannel CriarCanal()
    {
        if (_sharedChannel == null || _sharedChannel.IsClosed)
            _sharedChannel = CreateChannelAsync().Result;
        return _sharedChannel;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await Task.Run(() =>
            {
                _connection.CloseAsync();
                _connection.Dispose();
            });
        }
        Console.WriteLine("Conexão com RabbitMQ fechada assincronamente.");
    }
}