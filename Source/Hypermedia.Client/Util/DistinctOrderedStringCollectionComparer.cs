using System.Collections.Generic;
using System.Linq;
using Bluehands.Hypermedia.Client.Hypermedia;

namespace Bluehands.Hypermedia.Client.Util
{
    public class DistinctOrderedStringCollectionComparer : IEqualityComparer<IDistinctOrderedCollection<string>>
    {
        public bool Equals(IDistinctOrderedCollection<string> left, IDistinctOrderedCollection<string> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.SequenceEqual(right);
        }

        public int GetHashCode(IDistinctOrderedCollection<string> collection)
        {
            const int prime = 17;
            int multiplier = 1;
            int result = 0;
            foreach (var element in collection)
            {
                result += element.GetHashCode() * multiplier;
                multiplier *= prime;
            }

            return result;
        }
    }
}