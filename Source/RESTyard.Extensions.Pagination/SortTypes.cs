using System.Runtime.Serialization;

namespace RESTyard.Extensions.Pagination
{
    /// <summary>
    /// Represents the types of sorting that can be applied to a query result.
    /// </summary>
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