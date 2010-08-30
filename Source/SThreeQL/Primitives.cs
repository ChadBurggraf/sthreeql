//-----------------------------------------------------------------------
// <copyright file="Primitives.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides extensions to built-in primitive types, plus dates and strings.
    /// </summary>
    public static class Primitives
    {
        /// <summary>
        /// Converts a length of bytes to a friendly file size string.
        /// </summary>
        /// <param name="bytes">The bytes length to convert.</param>
        /// <returns>A friendly file size string.</returns>
        public static string ToFileSize(this long bytes)
        {
            const decimal KB = 1024;
            const decimal MB = KB * 1024;
            const decimal GB = MB * 1024;

            decimal size = Convert.ToDecimal(bytes);
            string suffix = " B";
            string format = "N0";

            if (bytes > GB)
            {
                size /= GB;
                suffix = " GB";
                format = "N2";
            }
            else if (bytes > MB)
            {
                size /= MB;
                suffix = " MB";
                format = "N1";
            }
            else if (bytes > KB)
            {
                size /= KB;
                suffix = " KB";
            }

            return size.ToString(format) + suffix;
        }

        /// <summary>
        /// Converts the given object to an integer and replaces it
        /// with the default value if it is null or 0.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <param name="defaultValue">The default value to use if empty.</param>
        /// <returns>An integer.</returns>
        public static int ToIntWithDefault(this object value, int defaultValue)
        {
            int ret = Convert.ToInt32(value ?? 0);

            if (ret == 0)
            {
                ret = defaultValue;
            }

            return ret;
        }

        /// <summary>
        /// Returns a string representation of the given DateTime object
        /// that conforms to ISO 8601 (in UTC).
        /// </summary>
        /// <param name="dateTime">The DateTime object to convert.</param>
        /// <returns>A given date as a string.</returns>
        public static string ToISO8601UTCString(this DateTime dateTime)
        {
            dateTime = dateTime.ToUniversalTime();
            return String.Format("{0:s}.{0:fff}Z", dateTime);
        }

        /// <summary>
        /// Returns a string representation of the given DateTime object
        /// that conforms to ISO 8601 (in UTC), replacing colons and periods
        /// with dashes for use in filenames.
        /// </summary>
        /// <param name="dateTime">The DateTime object to convert.</param>
        /// <returns>The given date as a string.</returns>
        public static string ToISO8601UTCPathSafeString(this DateTime dateTime)
        {
            return Regex.Replace(dateTime.ToISO8601UTCString(), @"[\.:]", "-");
        }

        /// <summary>
        /// Converts the given object to a string and replaces it with the given
        /// default value if it is null or empty.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <param name="defaultValue">The default value to use if empty.</param>
        /// <returns>The given object as a string.</returns>
        public static string ToStringWithDefault(this object value, string defaultValue)
        {
            string ret = (value ?? String.Empty).ToString();

            if (String.IsNullOrEmpty(ret))
            {
                ret = defaultValue;
            }

            return ret;
        }
    }
}
