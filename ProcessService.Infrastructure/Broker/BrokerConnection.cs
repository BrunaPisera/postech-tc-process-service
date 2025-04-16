using RabbitMQ.Client;

namespace ProcessService.Infrastructure.Broker
{
    public interface IBrokerConnection
    {
        IModel CreateChannel();
        void Dispose();
    }

    public class BrokerConnection : IBrokerConnection
    {
        private readonly IConnection _connection;

        public BrokerConnection()
        {
            var hostName = Environment.GetEnvironmentVariable("BROKER_HOSTNAME");
            var portString = Environment.GetEnvironmentVariable("BROKER_PORT");
            var userName = Environment.GetEnvironmentVariable("BROKER_USERNAME");
            var password = Environment.GetEnvironmentVariable("BROKER_PASSWORD");
            var virtualHost = Environment.GetEnvironmentVariable("BROKER_VIRTUALHOST");

            if (string.IsNullOrEmpty(hostName) ||
                string.IsNullOrEmpty(portString) ||
                string.IsNullOrEmpty(userName) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(virtualHost))
            {
                throw new Exception("Alguma variável de ambiente do broker está faltando.");
            }

            if (!int.TryParse(portString, out var port))
            {
                throw new Exception("BROKER_PORT inválido.");
            }

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
                VirtualHost = virtualHost
            };

            _connection = factory.CreateConnection();
        }

        public IModel CreateChannel()
        {
            return _connection.CreateModel();
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}