using System;
using System.Globalization;
using System.IO;

namespace RESTyard.WebApi.Extensions.Test.Helpers
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

        public static Stream ToStream(this string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
