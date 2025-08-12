using System.Runtime.Serialization;
using FunicularSwitch.Generators;

namespace RESTyard.Extensions.Pagination
{
    /// <summary>
    /// Represents the types of sorting that can be applied to a query result.
    /// </summary>
    [ExtendedEnum]
    public enum SortTypes
    {
        
        /// <summary>
        /// Represents no sorting applied to the query result.
        /// </summary>
        [EnumMember(Value = "None")]
        None,


        // Ascending sort.
        /// <summary>
        /// Represents sorting in ascending order.
        /// </summary>
        [EnumMember(Value = "Ascending")]
        Ascending,


        // Descending sort.
        /// <summary>
        /// Represents sorting in descending order.
        /// </summary>
        [EnumMember(Value = "Descending")]
        Descending
    }
}