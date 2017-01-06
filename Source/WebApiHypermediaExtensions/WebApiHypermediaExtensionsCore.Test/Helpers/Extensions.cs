using System;
using System.Globalization;

namespace WebApiHypermediaExtensionsCore.Test.Helpers
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
