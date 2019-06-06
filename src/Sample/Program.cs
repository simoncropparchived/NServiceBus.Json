using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Json;

class Program
{
    public static async Task Main()
    {
        var configuration = new EndpointConfiguration("JsonSerializerSample");
        configuration.UseSerialization<SystemJsonSerializer>();
        configuration.UseTransport<LearningTransport>();
        var endpoint = await Endpoint.Start(configuration);
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