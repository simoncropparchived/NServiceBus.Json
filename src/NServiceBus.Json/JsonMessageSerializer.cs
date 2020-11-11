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
        JsonWriterOptions writerOptions,
        JsonReaderOptions readerOptions,
        string contentType)
    {
        this.serializerOptions = serializerOptions;
        this.writerOptions = writerOptions;
        this.readerOptions = readerOptions;

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
        using var writer = new Utf8JsonWriter(stream,writerOptions);
        JsonSerializer.Serialize(writer, message, serializerOptions);
    }

    public object[] Deserialize(Stream stream, IList<Type> messageTypes)
    {
        if (messageTypes == null || !messageTypes.Any())
        {
            throw new Exception("NServiceBus.Json requires message types to be specified");
        }

        var buffer = ((MemoryStream)stream).ToArray();
        if (messageTypes.Count == 1)
        {
            return new[] {Deserialize(buffer, messageTypes[0])};
        }

        var rootTypes = FindRootTypes(messageTypes);
        return rootTypes.Select(rootType => Deserialize(buffer, rootType))
            .ToArray();
    }

    object Deserialize(byte[] buffer, Type type)
    {
        var reader = new Utf8JsonReader(buffer, readerOptions);
        return JsonSerializer.Deserialize(ref reader, type, serializerOptions)!;
    }

    static IEnumerable<Type> FindRootTypes(IEnumerable<Type> messageTypesToDeserialize)
    {
        Type? currentRoot = null;
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
    JsonWriterOptions writerOptions;
    JsonReaderOptions readerOptions;
}