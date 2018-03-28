//
// Options.cs
//
// Authors:
//  Jonathan Pryor <jpryor@novell.com>
//
// Copyright (C) 2008 Novell (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// The LINQ version just changes the implementation of
// OptionSet.Parse(IEnumerable<string>), and confers no semantic changes.
//
// A Getopt::Long-inspired option parsing library for C#.
//
// NDesk.Options.OptionSet is built upon a key/value table, where the
// key is a option format string and the value is a delegate that is 
// invoked when the format string is matched.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CommonUtilities;

namespace CommandLine.NDesk.Options
{
    public sealed class OptionValueCollection
    {
        private readonly List<string> _values = new List<string>();
        private readonly OptionContext _c;

        internal OptionValueCollection(OptionContext c)
        {
            _c = c;
        }

        #region ICollection<T>
        public void Add(string item) { _values.Add(item); }
        public void Clear() { _values.Clear(); }
        public int Count => _values.Count;

        #endregion



        #region IList<T>


        private void AssertValid(int index)
        {
            if (_c.Option == null)
                throw new InvalidOperationException("OptionContext.Option is null.");
            if (index >= _c.Option.MaxValueCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (_c.Option.OptionValueType == OptionValueType.Required &&
                index >= _values.Count)
                throw new OptionException($"Missing required value for option '{_c.OptionName}'.");
        }

        public string this[int index]
        {
            get
            {
                AssertValid(index);
                return index >= _values.Count ? null : _values[index];
            }
        }
        #endregion
    }

    public sealed class OptionContext
    {
        public OptionContext()
        {
            OptionValues = new OptionValueCollection(this);
        }

        public Option Option { get; set; }

        public string OptionName { get; set; }

        public int OptionIndex { get; set; }

        public OptionValueCollection OptionValues { get; }
    }

    public enum OptionValueType
    {
        None,
        Optional,
        Required
    }

    public abstract class Option
    {
        protected Option(string prototype, string description, int maxValueCount)
        {
            if (prototype == null)
                throw new ArgumentNullException(nameof(prototype));
            if (prototype.Length == 0)
                throw new ArgumentException("Cannot be the empty string.", nameof(prototype));
            if (maxValueCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxValueCount));

            Names = prototype.Split('|');
            Description = description;
            MaxValueCount = maxValueCount;
            OptionValueType = ParsePrototype();

            if (MaxValueCount == 0 && OptionValueType != OptionValueType.None)
                throw new ArgumentException(
                    "Cannot provide maxValueCount of 0 for OptionValueType.Required or " +
                    "OptionValueType.Optional.",
                    nameof(maxValueCount));
            if (OptionValueType == OptionValueType.None && maxValueCount > 1)
                throw new ArgumentException(
                    string.Format("Cannot provide maxValueCount of {0} for OptionValueType.None.", maxValueCount),
                    nameof(maxValueCount));
            if (Array.IndexOf(Names, "<>") >= 0 &&
                (Names.Length == 1 && OptionValueType != OptionValueType.None ||
                 Names.Length > 1 && MaxValueCount > 1))
                throw new ArgumentException(
                    "The default option handler '<>' cannot require values.",
                    nameof(prototype));
        }

        public string Description { get; }

        public OptionValueType OptionValueType { get; }

        public int MaxValueCount { get; }

        protected static T Parse<T>(string value, OptionContext c)
        {
            T t;
            try
            {
                t = (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception e)
            {
                throw new OptionException(
                    $"Could not convert string `{value}' to type {typeof(T).Name} for option `{c.OptionName}'.", e);
            }
            return t;
        }

        public string[] Names { get; }

        internal string[] ValueSeparators { get; private set; }

        private static readonly char[] NameTerminator = { '=', ':' };

        private OptionValueType ParsePrototype()
        {
            var type = '\0';
            var seps = new List<string>();
            for (var i = 0; i < Names.Length; ++i)
            {
                string name = Names[i];
                if (name.Length == 0)
                    throw new ArgumentException("Empty option names are not supported.", nameof(name));

                int end = name.IndexOfAny(NameTerminator);
                if (end == -1)
                    continue;
                Names[i] = name.Substring(0, end);
                if (type == '\0' || type == name[end])
                    type = name[end];
                else
                    throw new ArgumentException(
                        string.Format("Conflicting option types: '{0}' vs. '{1}'.", type, name[end]),
                        nameof(type));
                AddSeparators(name, end, seps);
            }

            if (type == '\0')
                return OptionValueType.None;

            if (MaxValueCount <= 1 && seps.Count != 0)
                throw new ArgumentException(
                    string.Format("Cannot provide key/value separators for Options taking {0} value(s).", MaxValueCount),
                    nameof(MaxValueCount));
            if (MaxValueCount <= 1) return type == '=' ? OptionValueType.Required : OptionValueType.Optional;

            switch (seps.Count)
            {
                case 0:
                    ValueSeparators = new[] { ":", "=" };
                    break;
                case 1 when seps[0].Length == 0:
                    ValueSeparators = null;
                    break;
                default:
                    ValueSeparators = seps.ToArray();
                    break;
            }

            return type == '=' ? OptionValueType.Required : OptionValueType.Optional;
        }

        private static void AddSeparators(string name, int end, ICollection<string> seps)
        {
            int start = -1;
            for (int i = end + 1; i < name.Length; ++i)
            {
                switch (name[i])
                {
                    case '{':
                        if (start != -1)
                            throw new ArgumentException(
                                string.Format("Ill-formed name/value separator found in \"{0}\".", name),
                                nameof(name));
                        start = i + 1;
                        break;
                    case '}':
                        if (start == -1)
                            throw new ArgumentException(
                                string.Format("Ill-formed name/value separator found in \"{0}\".", name),
                                nameof(name));
                        seps.Add(name.Substring(start, i - start));
                        start = -1;
                        break;
                    default:
                        if (start == -1)
                            seps.Add(name[i].ToString());
                        break;
                }
            }
            if (start != -1)
                throw new ArgumentException(
                    $"Ill-formed name/value separator found in \"{name}\".",
                    nameof(name));
        }

        public void Invoke(OptionContext c)
        {
            OnParseComplete(c);
            c.OptionName = null;
            c.Option = null;
            c.OptionValues.Clear();
        }

        protected abstract void OnParseComplete(OptionContext c);
    }

    public sealed class OptionException : Exception
    {
        public OptionException(string message)
            : base(message)
        {
        }

        public OptionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public sealed class OptionSet : KeyedCollection<string, Option>
    {
        protected override string GetKeyForItem(Option item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.Names != null && item.Names.Length > 0)
                return item.Names[0];
            // This should never happen, as it's invalid for Option to be
            // constructed w/o any names.
            throw new InvalidOperationException("Option has no names!");
        }

        protected override void InsertItem(int index, Option item)
        {
            base.InsertItem(index, item);
            AddImpl(item);
        }

        private void AddImpl(Option option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));
            var added = new List<string>(option.Names.Length);
            try
            {
                // KeyedCollection.InsertItem/SetItem handle the 0th name.
                for (var i = 1; i < option.Names.Length; ++i)
                {
                    Dictionary.Add(option.Names[i], option);
                    added.Add(option.Names[i]);
                }
            }
            catch (Exception)
            {
                foreach (string name in added)
                    Dictionary.Remove(name);
                throw;
            }
        }

        public new void Add(Option option)
        {
            base.Add(option);
        }

        private sealed class ActionOption : Option
        {
            private readonly Action<OptionValueCollection> _action;

            public ActionOption(string prototype, string description, int count, Action<OptionValueCollection> action)
                : base(prototype, description, count)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
            }

            protected override void OnParseComplete(OptionContext c)
            {
                _action(c.OptionValues);
            }
        }

        public void Add(string prototype, string description, Action<string> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            Option p = new ActionOption(prototype, description, 1,
                delegate (OptionValueCollection v) { action(v[0]); });
            base.Add(p);
        }

        private sealed class ActionOption<T> : Option
        {
            private readonly Action<T> _action;

            public ActionOption(string prototype, string description, Action<T> action)
                : base(prototype, description, 1)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
            }

            protected override void OnParseComplete(OptionContext c)
            {
                _action(Parse<T>(c.OptionValues[0], c));
            }
        }

        public void Add<T>(string prototype, string description, Action<T> action)
        {
            Add(new ActionOption<T>(prototype, description, action));
        }

        private static OptionContext CreateOptionContext()
        {
            return new OptionContext();
        }

        public List<string> Parse(IEnumerable<string> arguments)
        {
            OptionContext c = CreateOptionContext();
            c.OptionIndex = -1;
            var process = true;
            var unprocessed = new List<string>();
            Option def = Contains("<>") ? this["<>"] : null;
            foreach (string argument in arguments)
            {
                ++c.OptionIndex;
                if (argument == "--")
                {
                    process = false;
                    continue;
                }
                if (!process)
                {
                    Unprocessed(unprocessed, def, c, argument);
                    continue;
                }
                if (!Parse(argument, c))
                    Unprocessed(unprocessed, def, c, argument);
            }
            c.Option?.Invoke(c);
            return unprocessed;
        }

        private static void Unprocessed(ICollection<string> extra, Option def, OptionContext c, string argument)
        {
            if (def == null)
            {
                extra.Add(argument);
                return;
            }
            c.OptionValues.Add(argument);
            c.Option = def;
            c.Option.Invoke(c);
        }

        private readonly Regex _valueOption = new Regex(
            @"^(?<flag>--|-|/)(?<name>[^:=]+)((?<sep>[:=])(?<value>.*))?$");

        private bool GetOptionParts(string argument, out string flag, out string name, out string sep, out string value)
        {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));

            flag = name = sep = value = null;
            var m = _valueOption.Match(argument);
            if (!m.Success) return false;

            flag = m.Groups["flag"].Value;
            name = m.Groups["name"].Value;

            // ReSharper disable once InvertIf
            if (m.Groups["sep"].Success && m.Groups["value"].Success)
            {
                sep   = m.Groups["sep"].Value;
                value = m.Groups["value"].Value;
            }

            return true;
        }

        private bool Parse(string argument, OptionContext c)
        {
            if (c.Option != null)
            {
                ParseValue(argument, c);
                return true;
            }

            if (!GetOptionParts(argument, out string f, out string n, out string s, out string v))
                return false;

            if (!Contains(n)) return ParseBool(argument, n, c) || ParseBundledValue(f, n + s + v, c);

            var p = this[n];
            c.OptionName = f + n;
            c.Option = p;
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (p.OptionValueType)
            {
                case OptionValueType.None:
                    c.OptionValues.Add(n);
                    c.Option.Invoke(c);
                    break;
                case OptionValueType.Optional:
                case OptionValueType.Required:
                    ParseValue(v, c);
                    break;
            }
            return true;
        }

        private static void ParseValue(string option, OptionContext c)
        {
            if (option != null)
                foreach (string o in c.Option.ValueSeparators != null
                    ? option.Split(c.Option.ValueSeparators, StringSplitOptions.None)
                    : new[] { option })
                {
                    c.OptionValues.Add(o);
                }
            if (c.OptionValues.Count == c.Option.MaxValueCount ||
                c.Option.OptionValueType == OptionValueType.Optional)
                c.Option.Invoke(c);
            else if (c.OptionValues.Count > c.Option.MaxValueCount)
            {
                throw new OptionException($"Error: Found {c.OptionValues.Count} option values when expecting {c.Option.MaxValueCount}.");
            }
        }

        private bool ParseBool(string option, string n, OptionContext c)
        {
            string rn;
            if (n.Length < 1 || n[n.Length - 1] != '+' && n[n.Length - 1] != '-' ||
                !Contains(rn = n.Substring(0, n.Length - 1))) return false;

            var p = this[rn];
            string v = n[n.Length - 1] == '+' ? option : null;
            c.OptionName = option;
            c.Option = p;
            c.OptionValues.Add(v);
            p.Invoke(c);
            return true;
        }

        private bool ParseBundledValue(string f, string n, OptionContext c)
        {
            if (f != "-")
                return false;
            for (var i = 0; i < n.Length; ++i)
            {
                string opt = f + n[i];
                string rn = n[i].ToString();
                if (!Contains(rn))
                {
                    if (i == 0)
                        return false;
                    throw new OptionException($"Cannot bundle unregistered option '{opt}'.");
                }
                var p = this[rn];
                switch (p.OptionValueType)
                {
                    case OptionValueType.None:
                        Invoke(c, opt, n, p);
                        break;
                    case OptionValueType.Optional:
                    case OptionValueType.Required:
                        {
                            string v = n.Substring(i + 1);
                            c.Option = p;
                            c.OptionName = opt;
                            ParseValue(v.Length != 0 ? v : null, c);
                            return true;
                        }
                    default:
                        throw new InvalidOperationException("Unknown OptionValueType: " + p.OptionValueType);
                }
            }
            return true;
        }

        private static void Invoke(OptionContext c, string name, string value, Option option)
        {
            c.OptionName = name;
            c.Option = option;
            c.OptionValues.Add(value);
            option.Invoke(c);
        }

        private const int OptionWidth = 29;

        public void WriteOptionDescriptions(TextWriter o)
        {
            foreach (Option p in this)
            {
                var written = 0;
                if (!WriteOptionPrototype(o, p, ref written))
                    continue;

                if (written < OptionWidth)
                    o.Write(new string(' ', OptionWidth - written));
                else
                {
                    o.WriteLine();
                    o.Write(new string(' ', OptionWidth));
                }

                var indent = false;
                var prefix = new string(' ', OptionWidth + 2);
                foreach (string line in GetLines(GetDescription(p.Description)))
                {
                    if (indent)
                        o.Write(prefix);
                    o.WriteLine(line);
                    indent = true;
                }
            }
        }

        private static bool WriteOptionPrototype(TextWriter o, Option p, ref int written)
        {
            var names = p.Names;

            int i = GetNextOptionIndex(names, 0);
            if (i == names.Length)
                return false;

            if (names[i].Length == 1)
            {
                Write(o, ref written, "  -");
                Write(o, ref written, names[0]);
            }
            else
            {
                Write(o, ref written, "      --");
                Write(o, ref written, names[0]);
            }

            for (i = GetNextOptionIndex(names, i + 1);
                i < names.Length; i = GetNextOptionIndex(names, i + 1))
            {
                Write(o, ref written, ", ");
                Write(o, ref written, names[i].Length == 1 ? "-" : "--");
                Write(o, ref written, names[i]);
            }

            if (p.OptionValueType != OptionValueType.Optional && p.OptionValueType != OptionValueType.Required) return true;

            Write(o, ref written, " ");
            if (p.OptionValueType == OptionValueType.Optional)
            {
                Write(o, ref written, "[");
            }
            Write(o, ref written, "<" + GetArgumentName(0, p.MaxValueCount, p.Description) + '>');
            string sep = p.ValueSeparators != null && p.ValueSeparators.Length > 0
                ? p.ValueSeparators[0]
                : " ";
            for (var c = 1; c < p.MaxValueCount; ++c)
            {
                Write(o, ref written, sep + GetArgumentName(c, p.MaxValueCount, p.Description));
            }
            if (p.OptionValueType == OptionValueType.Optional)
            {
                Write(o, ref written, "]");
            }
            return true;
        }

        private static int GetNextOptionIndex(IReadOnlyList<string> names, int i)
        {
            while (i < names.Count && names[i] == "<>")
            {
                ++i;
            }
            return i;
        }

        private static void Write(TextWriter o, ref int n, string s)
        {
            n += s.Length;
            o.Write(s);
        }

        private static string GetArgumentName(int index, int maxIndex, string description)
        {
            if (description == null)
                return maxIndex == 1 ? "VALUE" : "VALUE" + (index + 1);
            var nameStart = maxIndex == 1 ? new[] { "{0:", "{" } : new[] { "{" + index + ":" };
            foreach (string t in nameStart)
            {
                int start, j = 0;
                do
                {
                    start = description.IndexOf(t, j, StringComparison.Ordinal);
                } while (start >= 0 && j != 0 && description[j++ - 1] == '{');
                if (start == -1)
                    continue;
                int end = description.IndexOf("}", start, StringComparison.Ordinal);
                if (end == -1)
                    continue;
                return description.Substring(start + t.Length, end - start - t.Length);
            }
            return maxIndex == 1 ? "VALUE" : "VALUE" + (index + 1);
        }

        private static string GetDescription(string description)
        {
            if (description == null)
                return string.Empty;
            StringBuilder sb = StringBuilderCache.Acquire(description.Length);
            int start = -1;
            for (var i = 0; i < description.Length; ++i)
            {
                switch (description[i])
                {
                    case '{':
                        if (i == start)
                        {
                            sb.Append('{');
                            start = -1;
                        }
                        else if (start < 0)
                            start = i + 1;
                        break;
                    case '}':
                        if (start < 0)
                        {
                            if (i + 1 == description.Length || description[i + 1] != '}')
                                throw new InvalidOperationException("Invalid option description: " + description);
                            ++i;
                            sb.Append("}");
                        }
                        else
                        {
                            sb.Append(description.Substring(start, i - start));
                            start = -1;
                        }
                        break;
                    case ':':
                        if (start < 0)
                            goto default;
                        start = i + 1;
                        break;
                    default:
                        if (start < 0)
                            sb.Append(description[i]);
                        break;
                }
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        private static IEnumerable<string> GetLines(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                yield return string.Empty;
                yield break;
            }

            description = description.Trim();
            int length = 80 - OptionWidth - 1;
            int start = 0, end;
            do
            {
                end = GetLineEnd(start, length, description);
                char c = description[end - 1];
                if (char.IsWhiteSpace(c))
                    --end;
                bool writeContinuation = end != description.Length && !IsEolChar(c);
                string line = description.Substring(start, end - start) +
                              (writeContinuation ? "-" : "");
                yield return line;
                start = end;
                if (char.IsWhiteSpace(c))
                    ++start;
                length = 80 - OptionWidth - 2 - 1;
            } while (end < description.Length);
        }

        private static bool IsEolChar(char c)
        {
            return !char.IsLetterOrDigit(c);
        }

        private static int GetLineEnd(int start, int length, string description)
        {
            int end = Math.Min(start + length, description.Length);
            int sep = -1;
            for (int i = start + 1; i < end; ++i)
            {
                if (description[i] == '\n')
                    return i + 1;
                if (IsEolChar(description[i]))
                    sep = i + 1;
            }
            if (sep == -1 || end == description.Length)
                return end;
            return sep;
        }
    }
}
