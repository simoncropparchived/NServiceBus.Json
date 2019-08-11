using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using NServiceBus;
using NServiceBus.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

class JsonMessageSerializer :
    IMessageSerializer
{
    public JsonMessageSerializer(
        JsonSerializerOptions serializerOptions,
        string contentType)
    {
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
        var bytes = JsonSerializer.SerializeToUtf8Bytes(message, serializerOptions);
        stream.Write(bytes, 0, bytes.Length);
    }

    public object[] Deserialize(Stream stream, IList<Type> messageTypes)
    {
        if (messageTypes == null || !messageTypes.Any())
        {
            throw new Exception("NServiceBus.Json requires message types to be specified");
        }

        var buffer = ((MemoryStream)stream).ToArray();
        var rootTypes = FindRootTypes(messageTypes);
        return rootTypes.Select(rootType => JsonSerializer.Deserialize(buffer, rootType, serializerOptions))
            .ToArray();
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