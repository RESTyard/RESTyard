using System.ComponentModel.DataAnnotations;

namespace WebApiHypermediaExtensionsCore.Util.Repository
{
    public class SortParameter<T>
        where T : struct
    {
        public SortParameter()
        {
        }

        // copy constructor
        public SortParameter(SortParameter<T> other)
        {
            PropertyName = other.PropertyName;
            SortType = other.SortType;
        }

        // The property name of the queried enity.
        public T? PropertyName { get; set; }

        // Sort type to be preformed.
        [EnumDataType(typeof(SortTypes))]
        public SortTypes SortType { get; set; }
    }
}