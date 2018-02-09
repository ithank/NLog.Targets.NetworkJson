using System;
using System.Globalization;

namespace NLog.Targets.NetworkJSON.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string ToPushMessage(this Exception ex)
        {
            return ($"{ex.GetType().Name}: {ex.Message}");
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return (string.IsNullOrEmpty(str));
        }

        public static bool CompareNoCase(this string stringA, string stringB)
        {
            if (string.Compare(stringA, stringB, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return (true);
            }
            return (false);
        }

        public static string SafeTrim(this string str)
        {
            if (str.IsNullOrEmpty()) return string.Empty;
            return (str.Trim());
        }

        public static string SafeToUpper(this string str)
        {
            if (str.IsNullOrEmpty()) return string.Empty;
            return (str.ToUpper());
        }

        public static string SafeTrimUpper(this string str)
        {
            if (str.IsNullOrEmpty()) return string.Empty;

            return (str.Trim().ToUpper());
        }

        public static bool IsSameCommandLineArg(this string arg, string argExpected)
        {
            if (argExpected.StartsWith("/") || argExpected.StartsWith("-"))
            {
                argExpected = argExpected.Substring(1);
            }

            if (arg.StartsWith("/") || arg.StartsWith("-"))
            {
                arg = arg.Substring(1);
            }

            return (arg.CompareNoCase(argExpected));
        }

        public static bool StartsWithCommandLineArg(this string arg, string argExpected)
        {
            return arg.SafeToUpper().StartsWith($"/{argExpected.SafeToUpper()}") || arg.SafeToUpper().StartsWith($"-{argExpected.SafeToUpper()}");
        }

        public static string SafeToCamelCase(this string str)
        {
            if (str == null || str.Length < 2) { return str; }

            // Split the string into words.
            string[] words = str.Replace("_", " ").Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = words[0].ToLower();
            for (int i = 1; i < words.Length; i++)
            {
                result +=
                    words[i].Substring(0, 1).ToUpper() +
                    words[i].Substring(1).ToLower();
            }

            return result;

        }
        public static string SafeToTitleCase(this string str)
        {
            if (str == null || str.Length < 2) { return str; }

            // Split the string into words.
            string[] words = str.Replace("_", " ").ToLower().Split(new char[] { });

            // Combine the words.
            string result = string.Join(" ", words);

            // Creates a TextInfo based on the "en-US" culture.
            TextInfo ti = new CultureInfo("en-US", false).TextInfo;

            result = ti.ToTitleCase(result).Replace(" ", String.Empty);

            return result;

        }

        public static string SafeLowerCaseFirst(this string str)
        {
            if (str == null || str.Length < 2) { return str; }
            return str.Substring(0, 1).ToLower() + str.Substring(1);
        }
    }
}
