using System;
using NServiceBus.MessageInterfaces;
using NServiceBus.Serialization;
using NServiceBus.Settings;

namespace NServiceBus.Json
{
    /// <summary>
    /// Defines the capabilities of the System.Text.Json serializer
    /// </summary>
    public class SystemJsonSerializer :
        SerializationDefinition
    {
        /// <summary>
        /// <see cref="SerializationDefinition.Configure"/>
        /// </summary>
        public override Func<IMessageMapper, IMessageSerializer> Configure(ReadOnlySettings settings)
        {
            Guard.AgainstNull(settings, nameof(settings));
            return mapper =>
            {
                var options = settings.GetOptions();
                var readerOptions = settings.GetReaderOptions();
                var writerOptions = settings.GetWriterOptions();
                var contentTypeKey = settings.GetContentTypeKey();
                return new JsonMessageSerializer(options, writerOptions, readerOptions, contentTypeKey);
            };
        }
    }
}