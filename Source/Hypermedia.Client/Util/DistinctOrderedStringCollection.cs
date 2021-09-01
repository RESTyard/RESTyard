using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bluehands.Hypermedia.Client.Hypermedia;

namespace Bluehands.Hypermedia.Client.Util
{
    public class DistinctOrderedStringCollection : Collection<string>, IDistinctOrderedCollection<string>
    {
        public DistinctOrderedStringCollection(params string[] classes)
        {
            this.Build(classes);
        }

        public DistinctOrderedStringCollection(IEnumerable<string> classes)
        {
            this.Build(classes);
        }

        private void Build(IEnumerable<string> classes)
        {
            foreach (var c in classes)
            {
                this.AddInternal(c);
            }
        }

        private void AddInternal(string item)
        {
            var actualIndex = this.BinarySearch(item);
            var alreadyExists = actualIndex < this.Count && this[actualIndex] == item;
            if (!alreadyExists)
            {
                this.InsertItem(actualIndex, item);
            }
        }

        protected int BinarySearch(string item)
        {
            int low = 0;
            int high = this.Count - 1;
            while (low <= high)
            {
                int i = (low + high) / 2;

                var comp = string.Compare(this[i], item, StringComparison.Ordinal);

                if (comp == 0)
                {
                    return i;
                }

                if (comp < 0)
                {
                    low = i + 1;
                }
                else
                {
                    high = i - 1;
                }
            }

            return low;
        }
    }
}