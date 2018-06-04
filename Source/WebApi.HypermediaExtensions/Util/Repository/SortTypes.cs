﻿using System.ComponentModel;
using System.Runtime.Serialization;
using WebApi.HypermediaExtensions.Util.Enum;

namespace WebApi.HypermediaExtensions.Util.Repository
{
    [TypeConverter(typeof(AttributedEnumTypeConverter<SortTypes>))]
    public enum SortTypes
    {

        // No sorting.
        [EnumMember(Value = "None")] None,


        // Ascending sort.
        [EnumMember(Value = "Ascending")] Ascending,


        // Descending sort.
        [EnumMember(Value = "Descending")] Descending
    }
}