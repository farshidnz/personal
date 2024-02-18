using System;

namespace Cashrewards3API.Extensions
{
    public static class StringExtension
    {
        public static string ReverseString(this string source)
        {
            char[] toArray = source.ToCharArray();
            Array.Reverse(toArray);
            return new string(toArray);
        }

        /// <summary>
        /// Returns either an integer if the string given can be converted to one or the default value provided if it cannot.
        /// The potential int substring is matched via a regex pattern
        /// </summary>
        /// <param name="source">string that contains the potential integer</param>
        /// <param name="regex">regex to match the substring containing the potential integer</param>
        /// <param name="defaultValue">default value to return if an int cannot be parsed from the source string</param>
        /// <returns>int or null/provided default value of int or null</returns>
        public static int? ToIntOrDefault(this string source, System.Text.RegularExpressions.Regex regex, int? defaultValue = null) {
            int number;
            System.Text.RegularExpressions.Match match = regex.Match(source ?? "");
            return (match.Success && int.TryParse(match.Value, out number)) ? number : defaultValue;
        }

        /// <summary>
        /// Returns either an integer if the string given can be converted to one or the default value provided if it cannot.
        /// The potential int substring is matched via a regex pattern.
        /// </summary>
        /// <param name="source">string that contains the potential integer</param>
        /// <param name="regex">regex to match the substring containing the potential integer</param>
        /// <param name="defaultValue">default value to return if an int cannot be parsed from the source string</param>
        /// <returns>int or provided default value of int</returns>
        public static int ToIntOrDefaultInt(this string source, System.Text.RegularExpressions.Regex regex, int defaultValue)
        {
            int number;
            System.Text.RegularExpressions.Match match = regex.Match(source ?? "");
            return (match.Success && int.TryParse(match.Value, out number)) ? number : defaultValue;
        }

        /// <summary>
        /// Returns either an integer if the string given can be converted to one or the default value provided if it cannot.
        /// </summary>
        /// <param name="source">string to try and convert to an integer</param>
        /// <param name="defaultValue">default value to return if an int cannot be parsed from the source string</param>
        /// <returns>int or null</returns>
        public static int? ToIntOrDefault(this string source, int? defaultValue = null)
        {
            return source.ToIntOrDefault(new System.Text.RegularExpressions.Regex(@".*"), defaultValue);
        }

        /// <summary>
        /// Returns either an integer if the string given can be converted to one or the default int value provided if it cannot.
        /// </summary>
        /// <param name="source">string to try and convert to an integer</param>
        /// <param name="defaultValue">default value to return if an int cannot be parsed from the source string</param>
        /// <returns>int or provided default value of int</returns>
        public static int ToIntOrDefaultInt(this string source, int defaultValue)
        {
            return source.ToIntOrDefaultInt(new System.Text.RegularExpressions.Regex(@".*"), defaultValue);
        }

        /// <summary>
        /// Returns either an integer if the string given can be converted to one or null if it cannot.
        /// The potential int substring is matched via a regex pattern
        /// </summary>
        /// <param name="source">string that contains the potential integer</param>
        /// <param name="regex">regex to match the substring containing the potential integer</param>
        /// <returns>int or null</returns>
        public static int? ToIntOrNull(this string source, System.Text.RegularExpressions.Regex regex)
        {
            return source.ToIntOrDefault(regex, null);
        }

        /// <summary>
        /// Returns either an integer if the string given can be converted to one or null if it cannot.
        /// </summary>
        /// <param name="source">string to try and convert to an integer</param>
        /// <returns>int or null</returns>
        public static int? ToIntOrNull(this string source)
        {
            return source.ToIntOrDefault(null);
        }

        /// <summary>
        /// Returns either an integer if the string given can be converted to one or 0 if it cannot.
        /// The potential int substring is matched via a regex pattern
        /// </summary>
        /// <param name="source">string that contains the potential integer</param>
        /// <param name="regex">regex to match the substring containing the potential integer</param>
        /// <returns>int or 0</returns>

        public static int ToIntOrZero(this string source, System.Text.RegularExpressions.Regex regex)
        {
            return source.ToIntOrDefaultInt(regex, 0);
        }

        /// <summary>
        /// Returns either an integer if the string given can be converted to one or 0 if it cannot.
        /// </summary>
        /// <param name="source">string to try and convert to an integer</param>
        /// <returns>int or 0</returns>
        public static int ToIntOrZero(this string source)
        {
            return source.ToIntOrDefaultInt(0);
        }
    }
}