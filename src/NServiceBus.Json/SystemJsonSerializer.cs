using NServiceBus.MessageInterfaces;
using NServiceBus.Serialization;
using NServiceBus.Settings;

namespace NServiceBus.Json;

/// <summary>
/// Defines the capabilities of the System.Text.Json serializer
/// </summary>
[Obsolete("The NServiceBus.Json package has been incorporated into NServiceBus version 8.1.0. This package will not be updated. See https://docs.particular.net/nservicebus/upgrades/community-system-json to upgrade.", false)]
public class SystemJsonSerializer :
    SerializationDefinition
{
    /// <summary>
    /// <see cref="SerializationDefinition.Configure"/>
    /// </summary>
    public override Func<IMessageMapper, IMessageSerializer> Configure(IReadOnlySettings settings) =>
        _ =>
        {
            var options = settings.GetOptions();
            var readerOptions = settings.GetReaderOptions();
            var writerOptions = settings.GetWriterOptions();
            var contentTypeKey = settings.GetContentTypeKey();
            return new JsonMessageSerializer(options, writerOptions, readerOptions, contentTypeKey);
        };
}