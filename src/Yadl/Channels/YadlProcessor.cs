using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Options;
using Yadl.Abstractions;

namespace Yadl.Channels
{
    public class YadlProcessor : IYadlProcessor
    {
        public Channel<YadlMessage> Channel { get; set; }
        public ChannelReader<YadlMessage> ChannelReader { get; }
        public ChannelWriter<YadlMessage> ChannelWriter { get; }
        public ConcurrentBag<YadlMessage> Messages { get; }

        private readonly YadlLoggerOptions _options;

        public YadlProcessor(IOptions<YadlLoggerOptions> options) : this(options.Value)
        {
        }

        public YadlProcessor(YadlLoggerOptions options)
        {
            _options = options;

            if (_options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Channel = System.Threading.Channels.Channel.CreateUnbounded<YadlMessage>(
                new UnboundedChannelOptions
                {
                    SingleReader = false,
                    SingleWriter = true
                });

            Messages = new ConcurrentBag<YadlMessage>();

            ChannelReader = Channel.Reader;
            ChannelWriter = Channel.Writer;
        }
    }
}