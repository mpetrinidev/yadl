using System.Threading.Channels;

namespace Yadl
{
    public class YadlProviderOptions
    {
        public int Capacity { get; set; }
        public BoundedChannelFullMode ChannelFullMode { get; set; }
    }
}