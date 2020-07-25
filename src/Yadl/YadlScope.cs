using System;
using System.Collections.Generic;
using System.Threading;

namespace Yadl
{
    public class YadlScope
    {
        private YadlScope(IEnumerable<KeyValuePair<string, object>> additionalFields)
        {
            AdditionalFields = additionalFields;
        }

        public YadlScope? Parent { get; private set; }

        public IEnumerable<KeyValuePair<string, object>> AdditionalFields { get; }

        private static readonly AsyncLocal<YadlScope?> _value = new AsyncLocal<YadlScope?>();

        public static YadlScope? Current
        {
            get => _value.Value;
            set => _value.Value = value;
        }

        public static IDisposable Push(IEnumerable<KeyValuePair<string, object>> additionalFields)
        {
            var parent = Current;
            Current = new YadlScope(additionalFields) {Parent = parent};

            return new DisposableScope();
        }

        private class DisposableScope : IDisposable
        {
            public void Dispose()
            {
                Current = Current?.Parent;
            }
        }
    }
}