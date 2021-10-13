using NServiceBus;
using NServiceBus.Json;

class Program
{
    public static async Task Main()
    {
        var endpointConfiguration = new EndpointConfiguration("JsonSerializerSample");
        #region usage
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        #endregion
        endpointConfiguration.UseTransport<LearningTransport>();
        var endpoint = await Endpoint.Start(endpointConfiguration);
        var message = new MyMessage
        {
            DateSend = DateTime.Now,
        };
        await endpoint.SendLocal(message);
        Console.WriteLine("Press any key to stop program");
        Console.Read();
        await endpoint.Stop();
    }
}