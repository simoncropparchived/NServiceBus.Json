using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using NServiceBus;
using NServiceBus.MessageInterfaces;
using NServiceBus.Serialization;

class JsonMessageSerializer :
    IMessageSerializer
{
    IMessageMapper messageMapper;

    public JsonMessageSerializer(
        IMessageMapper messageMapper,
        JsonSerializerOptions serializerOptions,
        string contentType)
    {
        this.messageMapper = messageMapper;
        this.serializerOptions = serializerOptions;

        if (contentType == null)
        {
            ContentType = ContentTypes.Json;
        }
        else
        {
            ContentType = contentType;
        }
    }

    public void Serialize(object message, Stream stream)
    {
        try
        {
            var bytes = System.Text.Json.Serialization.JsonSerializer.ToBytes(message, serializerOptions);
            stream.Write(bytes, 0, bytes.Length);
        }
        catch (TypeAccessException exception)
        {
            throw new Exception($"Types need to be public. Type: {message.GetType().FullName}", exception);
        }
    }

    public object[] Deserialize(Stream stream, IList<Type> messageTypes)
    {
        if (messageTypes == null || !messageTypes.Any())
        {
            throw new Exception("NServiceBus.Json requires message types to be specified");
        }

        var buffer = ((MemoryStream)stream).ToArray();
        var rootTypes = FindRootTypes(messageTypes);
        return rootTypes.Select(rootType =>
            {
                var messageType = GetMappedType(rootType);
                try
                {
                    return System.Text.Json.Serialization.JsonSerializer.Parse(buffer, rootType, serializerOptions);
                }
                catch (TypeAccessException exception)
                {
                    throw new Exception($"Types need to be public. Type: {messageType.FullName}", exception);
                }
            })
            .ToArray();
    }

    Type GetMappedType(Type serializedType)
    {
        return messageMapper.GetMappedTypeFor(serializedType) ?? serializedType;
    }


    static IEnumerable<Type> FindRootTypes(IEnumerable<Type> messageTypesToDeserialize)
    {
        Type currentRoot = null;
        foreach (var type in messageTypesToDeserialize)
        {
            if (currentRoot == null)
            {
                currentRoot = type;
                yield return currentRoot;
                continue;
            }

            if (!type.IsAssignableFrom(currentRoot))
            {
                currentRoot = type;
                yield return currentRoot;
            }
        }
    }

    public string ContentType { get; }

    JsonSerializerOptions serializerOptions;
}