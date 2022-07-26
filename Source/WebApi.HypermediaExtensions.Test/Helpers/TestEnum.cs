﻿using System;
using System.Runtime.Serialization;

namespace RESTyard.WebApi.Extensions.Test.Helpers
{
    public enum TestEnum
    {
        None,
        Value1,
        Value2
    }

    public enum TestEnumWithNames
    {
        [EnumMember(Value = "NoneRename")]
        None,

        [EnumMember(Value = "Value1Rename")]
        Value1,

        [EnumMember(Value = "Value2Rename")]
        Value2
    }
}