using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Logging
{
    public class NullExternalScopeProvider : IExternalScopeProvider
    {
        private NullExternalScopeProvider()
        {
        }

        public static IExternalScopeProvider Instance { get; } = new NullExternalScopeProvider();

        void IExternalScopeProvider.ForEachScope<TState>(Action<object, TState> callback, TState state)
        {
        }

        IDisposable IExternalScopeProvider.Push(object state)
        {
            return NullScope.Instance;
        }
    }
}