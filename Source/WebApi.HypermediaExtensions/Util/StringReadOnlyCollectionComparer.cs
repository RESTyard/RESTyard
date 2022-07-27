using System;
using System.Collections.Generic;
using System.Linq;

namespace RESTyard.AspNetCore.Util
{
    public class StringReadOnlyCollectionComparer : IEqualityComparer<IReadOnlyCollection<string>>
    {
        public bool Equals(IReadOnlyCollection<string> x, IReadOnlyCollection<string> y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            if (x.Count != y.Count)
            {
                return false;
            }

            return !x.Except(y).Any();
        }

        public int GetHashCode(IReadOnlyCollection<string> obj)
        {
            return 0;
        }
    }
}