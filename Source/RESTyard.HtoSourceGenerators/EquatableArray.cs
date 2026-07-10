using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Wraps an <see cref="ImmutableArray{T}"/> to provide value equality,
/// required for correct incremental generator caching.
/// </summary>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    private readonly ImmutableArray<T> _array;

    public EquatableArray(ImmutableArray<T> array) => _array = array;

    public EquatableArray(IEnumerable<T> items) => _array = items.ToImmutableArray();

    public int Length => _array.Length;

    public bool IsEmpty => _array.IsDefaultOrEmpty;

    public T this[int index] => _array[index];

    public bool Equals(EquatableArray<T> other)
    {
        if (_array.Length != other._array.Length)
        {
            return false;
        }

        for (var i = 0; i < _array.Length; i++)
        {
            if (!_array[i].Equals(other._array[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
        => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            foreach (var item in _array)
                hash = hash * 31 + item.GetHashCode();
            return hash;
        }
    }

    public ImmutableArray<T>.Enumerator GetEnumerator() => _array.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
        => ((IEnumerable<T>)_array).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)_array).GetEnumerator();
}
