using System;
using System.Linq;
using CacheUtils.DataDumperImport.IO;

namespace CacheUtils.DataDumperImport.FauxRegex
{
    internal static class RegexDecisionTree
    {
        internal static (EntryType Type, string Key, string Value) GetEntryType(string s)
        {
            s = s.Trim().TrimEnd(',');

            int fatArrowPos = s.IndexOf("=>", StringComparison.Ordinal);
            return fatArrowPos != -1
                ? GetEntryTypeFatArrow(s, fatArrowPos)
                : GetEntryTypeNoArrow(s);
        }

        private static (EntryType Type, string Key, string Value) GetEntryTypeNoArrow(string s)
        {
            int varPos = s.IndexOf("$VAR", StringComparison.Ordinal);
            return varPos != -1 ? GetEntryTypeVar(s) : GetEntryTypeNoVar(s);
        }

        private static (EntryType Type, string Key, string Value) GetEntryTypeNoVar(string s)
        {
            s = s.TrimEnd(';');

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (s == "}") return (EntryType.EndBraces, null, null);
            if (s == "bless( {") return (EntryType.OpenBraces, null, null);

            int endBracePos = s.IndexOf("}, 'Bio::", StringComparison.Ordinal);
            if (endBracePos != -1) return GetEntryTypeDataPos(s, endBracePos + 4);

            s = s.Trim('\'');
            if (OnlyDigits(s)) return (EntryType.DigitKey, s, null);

            throw new NotImplementedException($"Unable to match the non-$VAR regexes: [{s}]");
        }

        private static (EntryType Type, string Key, string Value) GetEntryTypeDataPos(string s,
            int afterFirstQuote)
        {
            return (EntryType.EndBracesWithDataType, GetForwardString(s, afterFirstQuote), null);
        }

        private static (EntryType Type, string Key, string Value) GetEntryTypeVar(string s)
        {
            if (!s.EndsWith(" = {")) throw new NotImplementedException("Unable to match the $VAR regexes: [{s}]");

            int spacePos = s.IndexOf(' ');
            return (EntryType.RootObjectKeyValue, s.Substring(0, spacePos), null);
        }

        private static (EntryType, string Key, string Value) GetEntryTypeFatArrow(string s, int fatArrowPos)
        {
            string key = GetKey(s, fatArrowPos - 2);

            int firstPosAfterFatArrow = fatArrowPos + 3;
            if (s[firstPosAfterFatArrow] == '\'') return GetEntryTypeStringKeyValue(s, firstPosAfterFatArrow + 1, key);
            if (s[s.Length - 1] == '{') return (EntryType.ObjectKeyValue, key, null);

            string afterFatArrow = s.Substring(firstPosAfterFatArrow);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (afterFatArrow == "undef") return (EntryType.UndefKeyValue, key, null);
            if (afterFatArrow == "{}") return (EntryType.EmptyValueKeyValue, key, null);
            if (afterFatArrow == "[]") return (EntryType.EmptyListKeyValue, key, null);
            if (afterFatArrow.StartsWith("$VAR")) return (EntryType.ReferenceStringKeyValue, key, afterFatArrow);

            if (s[firstPosAfterFatArrow] == '[') return (EntryType.ListObjectKeyValue, key, null);
            if (OnlyDigits(afterFatArrow)) return (EntryType.DigitKeyValue, key, afterFatArrow);

            throw new NotImplementedException();
        }

        private static (EntryType, string Key, string Value) GetEntryTypeStringKeyValue(string s, int afterFirstQuote, string key)
        {
            int secondQuotePos = s.IndexOf('\'', afterFirstQuote);

            return secondQuotePos == -1
                ? (EntryType.MultiLineKeyValue, key, s.Substring(afterFirstQuote))
                : (EntryType.StringKeyValue, key, s.Substring(afterFirstQuote,
                    secondQuotePos - afterFirstQuote));
        }

        private static string GetKey(string s, int secondQuotePos)
        {
            int afterFirstQuote = s.LastIndexOf('\'', secondQuotePos - 1) + 1;
            return s.Substring(afterFirstQuote, secondQuotePos - afterFirstQuote);
        }

        private static string GetForwardString(string s, int afterFirstQuote)
        {
            int secondQuotePos = s.IndexOf('\'', afterFirstQuote);
            string result = s.Substring(afterFirstQuote, secondQuotePos - afterFirstQuote);
            return result;
        }

        internal static bool OnlyDigits(string s) => s.All(c => char.IsDigit(c) || c == '-');
    }
}
