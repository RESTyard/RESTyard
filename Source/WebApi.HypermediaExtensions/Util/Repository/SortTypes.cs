using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using RESTyard.WebApi.Extensions.Util.Enum;

namespace RESTyard.WebApi.Extensions.Util.Repository
{
    [TypeConverter(typeof(AttributedEnumTypeConverter<SortTypes>))]
    public enum SortTypes
    {

        // No sorting.
        [EnumMember(Value = "None")]
        None,


        // Ascending sort.
        [EnumMember(Value = "Ascending")]
        Ascending,


        // Descending sort.
        [EnumMember(Value = "Descending")]
        Descending
    }
}