﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalLogStorage.ExtensionMethods
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
    }
}
