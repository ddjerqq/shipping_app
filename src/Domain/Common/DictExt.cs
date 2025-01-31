using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Domain.Common;

public static class DictExt
{
    public static TValue? GetOrAdd<TKey, TValue>(
        this Dictionary<TKey, TValue> dict, TKey key, TValue? value)
        where TKey : notnull
    {
        ref var valueRef = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out var exists);

        if (exists)
            return valueRef;

        valueRef = value;
        return value;
    }

    public static bool TryUpdate<TKey, TValue>(
        this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        where TKey : notnull
    {
        ref var valueRef = ref CollectionsMarshal.GetValueRefOrNullRef(dict, key);

        if (Unsafe.IsNullRef(ref valueRef))
            return false;

        valueRef = value;
        return true;
    }
}