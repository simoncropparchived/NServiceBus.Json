using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Json;
using Xunit;
using Xunit.Abstractions;

public class IntegrationTest :
    XunitApprovalBase
{
    [Fact]
    public async Task Run()
    {
        var configuration = new EndpointConfiguration("Test");
        configuration.UseTransport<LearningTransport>();
        configuration.UsePersistence<LearningPersistence>();
        configuration.UseSerialization<SystemJsonSerializer>();
        configuration.PurgeOnStartup(true);
        using (var resetEvent = new ManualResetEvent(false))
        {
            configuration.RegisterComponents(components => components.RegisterSingleton(resetEvent));
        
            var endpoint = await Endpoint.Start(configuration);
            await endpoint.SendLocal(new MyMessage {Property = "Value"});

            if (!resetEvent.WaitOne(TimeSpan.FromSeconds(10)))
            {
                throw new Exception("No Set received.");
            }
        }
    }

    public IntegrationTest(ITestOutputHelper output) : 
        base(output)
    {
    }
}

public class MyMessage :
    IMessage
{
    public string Property { get; set; }
}

class Handler :
    IHandleMessages<MyMessage>
{
    ManualResetEvent resetEvent;

    public Handler(ManualResetEvent resetEvent)
    {
        this.resetEvent = resetEvent;
    }

    public Task Handle(MyMessage message, IMessageHandlerContext context)
    {
        Assert.Equal("Value", message.Property);
        resetEvent.Set();
        return Task.CompletedTask;
    }
}