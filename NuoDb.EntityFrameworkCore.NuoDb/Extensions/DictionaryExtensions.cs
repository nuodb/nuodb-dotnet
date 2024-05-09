using System.Collections.Generic;
using System.Diagnostics;

namespace NuoDb.EntityFrameworkCore.NuoDb.Extensions
{
    [DebuggerStepThrough]
    internal static class DictionaryExtensions
    {
        public static TValue GetOrAddNew<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TKey key)
            where TValue : new()
        {
            if (!source.TryGetValue(key, out var value))
            {
                value = new TValue();
                source.Add(key, value);
            }

            return value;
        }
    }
}
