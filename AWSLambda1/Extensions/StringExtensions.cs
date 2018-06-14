using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AWSLambda1.Extensions
{
    public static class StringExtensions
    {
        public static bool GetDoublePub(this string ts, out double res)
        {
            res = double.NaN;
            if (ts.HasValuePub())
            {
                if (double.TryParse(ts, NumberStyles.Any, CultureInfo.InvariantCulture, out res))
                {
                    return true;
                }
            }

            return false;
        }

        ///<summary>
        /// Extension method that returns true if the string is not null and not empty and not just whitespace.
        /// </summary>
        public static bool HasValuePub(this string ts)
        {
            return !String.IsNullOrWhiteSpace(ts);
        }

        public static double GetDoublePub(this string ts)
        {
            if (ts.HasValuePub())
            {
                double val;
                if (double.TryParse(ts, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                {
                    return val;
                }
            }

            return double.NaN;
        }
    }
}
