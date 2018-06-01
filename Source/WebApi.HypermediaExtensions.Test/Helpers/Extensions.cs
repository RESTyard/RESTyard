using System;
using System.Globalization;

namespace WebApi.HypermediaExtensions.Test.Helpers
{
    public static class Extensions
    {
        public static string ToInvariantString(this IFormattable formattable)
        {
            return formattable.ToString(null, CultureInfo.InvariantCulture);
        }

        public static string ToStringZNotation(this IFormattable formattable)
        {
            return formattable.ToString("o", CultureInfo.InvariantCulture);
        }
    }
}
