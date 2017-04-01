using System.Collections.Generic;
using System.Linq;

namespace Hypermedia.Util
{
    public class StringCollectionComparer : IEqualityComparer<ICollection<string>>
    {
        public bool Equals(ICollection<string> x, ICollection<string> y)
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

        public int GetHashCode(ICollection<string> obj)
        {
            return 0;
        }
    }
}