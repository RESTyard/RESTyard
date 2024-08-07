﻿using System;

namespace RESTyard.Client.Extensions.SystemNetHttp
{
    public static class StringHelpers
    {
        public static string SurroundWithQuotes(string text)
        {
            const string doubleQuoteString = "\"";
            if (!text.StartsWith(doubleQuoteString))
            {
                text = doubleQuoteString + text;
            }

            if (!text.EndsWith(doubleQuoteString))
            {
                text = text + doubleQuoteString;
            }

            return text;
        }

        /// <summary>
        /// Removes double quotes (") from the beginning and the front of the given text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveSurroundingQuotes(string text)
        {
            const char doubleQuoteChar = '"';
            var unquoted = text.Trim(doubleQuoteChar);
            return unquoted;
        }
    }
}