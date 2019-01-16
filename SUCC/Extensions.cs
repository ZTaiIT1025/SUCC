﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SUCC
{
    internal static class Extensions
    {
        internal static string Quote(this string s)
            => '"' + s + '"';

        internal static bool IsQuoted(this string s)
            => s.Length > 1 && s[0] == '"' && s[s.Length - 1] == '"';

        internal static string UnQuote(this string s)
        {
            if (!s.IsQuoted()) throw new InvalidOperationException($"tried to unquote string {s} which is not quoted. Make sure you check for quotes before unquoting.");
            return s.Substring(1, s.Length - 2);
        }

        /// <summary> the number of spaces in the string that precede the first non-space character </summary>
        internal static int GetIndentationLevel(this string s)
            => s.TakeWhile(c => c == ' ').Count();
    }
}