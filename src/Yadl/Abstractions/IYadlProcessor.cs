using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Yadl.Abstractions
{
    public interface IYadlProcessor
    {
        Channel<YadlMessage> Channel { get; set; }
        ChannelReader<YadlMessage> ChannelReader { get; }
        ChannelWriter<YadlMessage> ChannelWriter { get; }
        ConcurrentBag<YadlMessage> Messages { get; }
    }
}