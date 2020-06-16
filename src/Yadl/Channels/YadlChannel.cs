using System.Threading.Channels;

namespace Yadl.Channels
{
    public class YadlPubSub
    {
        private readonly YadlProviderOptions _yadlProviderOptions;
        public readonly Channel<string> Channel;

        public YadlPubSub(YadlProviderOptions yadlProviderOptions)
        {
            _yadlProviderOptions = yadlProviderOptions;
            Channel = System.Threading.Channels.Channel.CreateBounded<string>(new BoundedChannelOptions(_yadlProviderOptions.Capacity)
            {
                FullMode = _yadlProviderOptions.ChannelFullMode
            });
        }
    }
}