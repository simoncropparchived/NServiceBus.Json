using System;
using NServiceBus;

public class MyMessage : IMessage
{
    public DateTime DateSend { get; set; }
}