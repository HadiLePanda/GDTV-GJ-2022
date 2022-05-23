using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GameJam
{
    public static class StringExtensions
    {
        // string to int (returns errVal if failed)
        public static int ToInt(this string value, int errVal = 0)
        {
            Int32.TryParse(value, out errVal);
            return errVal;
        }

        // string to long (returns errVal if failed)
        public static long ToLong(this string value, long errVal = 0)
        {
            Int64.TryParse(value, out errVal);
            return errVal;
        }

        // formatted text
        public static string ToBold(this string value)
        {
            return $"<b>{value}</b>";
        }

        /// <summary>
        /// Color a text, is formatted for UI
        /// </summary>
        /// <param name="text">The text to be colored</param>
        /// <param name="color">The desired color</param>
        /// <returns>Return a color-formatted string</returns>
        public static string GetColoredText(string text, Color color)
        {
            string hexColor = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hexColor}>{text}</color>";
        }

        /// <summary>
        /// Join a list of strings on a same line with a comma separator
        /// </summary>
        /// <param name="strings">The list of strings to join</param>
        /// <returns>Return line of string with each item separated by a comma</returns>
        public static string ToLinearList(IEnumerable<string> strings)
        {
            return string.Join(",", strings);
        }

        /// <summary>
        /// Join a list of strings on multiple lines
        /// </summary>
        /// <param name="strings">The list of strings to join</param>
        /// <returns>Return a single string representing each string in the list on multiple lines</returns>
        public static string ToLinesList(IEnumerable<string> strings)
        {
            var linesListSb = new StringBuilder();

            foreach (var item in strings)
            {
                linesListSb.AppendLine(item);
            }

            return linesListSb.ToString();
        }

        // string.GetHashCode is not guaranteed to be the same on all machines, but
        // we need one that is the same on all machines. simple and stupid:
        public static int GetStableHashCode(this string text)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in text)
                    hash = hash * 31 + c;
                return hash;
            }
        }
    }
}