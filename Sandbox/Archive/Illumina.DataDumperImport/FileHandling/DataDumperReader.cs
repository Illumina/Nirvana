using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Illumina.DataDumperImport.DataStructures;
using Illumina.VariantAnnotation.FileHandling;

namespace Illumina.DataDumperImport.FileHandling
{
    public sealed class DataDumperReader : IDisposable
    {
        #region members

        public ObjectKeyValue RootNode { get; private set; }

        private const string RootKeyName      = "root";

        private const string EndBracesTag    = "}";
        private const string EndBracesTag2   = "},";
        private const string EndBracesTag3   = "};";
        private const string EndBracketsTag  = "]";
        private const string EndBracketsTag2 = "],";
        private const string EndStringTag    = "'";
        private const string EndStringTag2   = "',";

        private const string EndBinaryTag  = "\0\0'";
        private const string EndBinaryTag2 = "\0\0',";

        private readonly Regex _binaryKeyValueRegex;
        private readonly Regex _dataTypeRegex;
        private readonly Regex _digitKeyRegex;
        private readonly Regex _digitKeyValueRegex;
        private readonly Regex _emptyListKeyValueRegex;
        private readonly Regex _emptyValueKeyValueRegex;
        private readonly Regex _listObjectKeyValueRegex;
        private readonly Regex _multiLineKeyValueRegex;
        private readonly Regex _objectKeyValueRegex;
        private readonly Regex _openBracesRegex;
        private readonly Regex _referenceStringKeyRegex;
        private readonly Regex _referenceStringKeyValueRegex;
        private readonly Regex _rootObjectKeyValueRegex;
        private readonly Regex _stringKeyRegex;
        private readonly Regex _stringKeyValueRegex;
        private readonly Regex _undefKeyValueRegex;

        private readonly StreamReader _reader;

        enum EntryType
        {
            BinaryKeyValue,
            DigitKeyValue,
            DigitKey,
            EmptyListKeyValue,
            EmptyValueKeyValue,
            EndBraces,
            ListObjectKeyValue,
            MultiLineKeyValue,
            ObjectKeyValue,
            OpenBraces,
            ReferenceStringKey,
            ReferenceStringKeyValue,
            RootObjectKeyValue,
            StringKey,
            StringKeyValue,
            UndefKeyValue
        }

        #endregion

        #region IDisposable

        private bool _isDisposed;

        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// protected implementation of Dispose pattern. 
        /// </summary>
        private void Dispose(bool disposing)
        {
            lock (this)
            {
                if (_isDisposed) return;

                if (disposing)
                {
                    // Free any other managed objects here. 
                    _reader.Close();
                }

                // Free any unmanaged objects here. 
                _isDisposed = true;
            }
        }

        #endregion

        // constructor
        public DataDumperReader(string filename)
        {
            // define our regular expressions
            _binaryKeyValueRegex          = new Regex("'([^']+)' => '\x1f\xfffd\x08",        RegexOptions.Compiled);
            _dataTypeRegex                = new Regex("}, '([^']+)' \\)",                    RegexOptions.Compiled);
            _digitKeyRegex                = new Regex("^\\s*([\\d\\.]+)(?:,?)\\s*$",         RegexOptions.Compiled);
            _digitKeyValueRegex           = new Regex("'([^']+)' => (\\d+)",                 RegexOptions.Compiled);
            _emptyListKeyValueRegex       = new Regex("'([^']+)' => \\[\\]",                 RegexOptions.Compiled);
            _emptyValueKeyValueRegex      = new Regex("'([^']+)' => \\{\\}",                 RegexOptions.Compiled);
            _listObjectKeyValueRegex      = new Regex("'([^']+)' => \\[",                    RegexOptions.Compiled);
            _multiLineKeyValueRegex       = new Regex("'([^']+)' => '([^']+)$",              RegexOptions.Compiled);
            _objectKeyValueRegex          = new Regex("'([^']+)' => (?:bless\\( )?{",        RegexOptions.Compiled);
            _openBracesRegex              = new Regex("bless\\( \\{",                        RegexOptions.Compiled);
            _referenceStringKeyRegex      = new Regex("^\\s*(\\$VAR\\d+->\\S+?)(?:,?)\\s*$", RegexOptions.Compiled);
            _referenceStringKeyValueRegex = new Regex("'([^']+)' => (\\$VAR\\S+)(?:,?)",     RegexOptions.Compiled);
            _rootObjectKeyValueRegex      = new Regex("\\$VAR\\d = {",                       RegexOptions.Compiled);
            _stringKeyRegex               = new Regex("^\\s*'([^']+)'(?:,?)\\s*$",           RegexOptions.Compiled);
            _stringKeyValueRegex          = new Regex("'([^']+)' => '([^']*)'",              RegexOptions.Compiled);
            _undefKeyValueRegex           = new Regex("'([^']+)' => undef",                  RegexOptions.Compiled);

            // start building the dumper hierarchy
            using (_reader = GZipUtilities.GetAppropriateStreamReader(filename))
            {
                BuildDumperHierarchy();
            }

            // dump the tree
            // Console.WriteLine(_rootNode);
        }

        /// <summary>
        /// poor man's JSON parser that handles the mangled Data::Dumper output
        /// </summary>
        private void BuildDumperHierarchy()
        {
            // Console.WriteLine("*** BuildDumperHierarchy ***");

            // grab the next line
            string line = _reader.ReadLine();

            if (line == null)
            {
                throw new ApplicationException("Expected a root object node, but no data was found.");
            }

            // retrieve the entry type
            EntryType entryType = GetEntryType(line);
            // Console.WriteLine(entryType);

            if (entryType != EntryType.RootObjectKeyValue)
            {
                throw new ApplicationException($"Expected a root object node, but found a {entryType} node.");
            }

            // assign the root node
            RootNode = new ObjectKeyValue(RootKeyName, GetObjectValue());
        }

        /// <summary>
        /// returns the entry type that is required to store the next object
        /// </summary>
        private EntryType GetEntryType(string s)
        {
            // undef
            var undefMatch = _undefKeyValueRegex.Match(s);
            if(undefMatch.Success) return EntryType.UndefKeyValue;

            // rootMatch
            var rootMatch = _rootObjectKeyValueRegex.Match(s);
            if (rootMatch.Success) return EntryType.RootObjectKeyValue;

            // emptyValueMatch
            var emptyValueMatch = _emptyValueKeyValueRegex.Match(s);
            if (emptyValueMatch.Success) return EntryType.EmptyValueKeyValue;

            // objectMatch
            var objectMatch = _objectKeyValueRegex.Match(s);
            if (objectMatch.Success) 
                return EntryType.ObjectKeyValue;

            // emptyListMatch
            var emptyListMatch = _emptyListKeyValueRegex.Match(s);
            if (emptyListMatch.Success) return EntryType.EmptyListKeyValue;

            // listObjectMatch
            var listObjectMatch = _listObjectKeyValueRegex.Match(s);
            if (listObjectMatch.Success) return EntryType.ListObjectKeyValue;

            // digitMatch
            var digitMatch = _digitKeyValueRegex.Match(s);
            if (digitMatch.Success) return EntryType.DigitKeyValue;

            // binaryMatch
            var binaryMatch = _binaryKeyValueRegex.Match(s);
            if (binaryMatch.Success) return EntryType.BinaryKeyValue;

            // stringMatch
            var stringMatch = _stringKeyValueRegex.Match(s);
            if (stringMatch.Success) return EntryType.StringKeyValue;

            // openBracesMatch
            var openBracesMatch = _openBracesRegex.Match(s);
            if (openBracesMatch.Success) return EntryType.OpenBraces;

            // endBracesMatch
            var endBracesMatch = _dataTypeRegex.Match(s);
            if (endBracesMatch.Success) return EntryType.EndBraces;

            string ws = s.Trim();
            if ((ws == EndBracesTag) || (ws == EndBracesTag2) || (ws == EndBracesTag3)) return EntryType.EndBraces;

            // referenceStringMatch
            var referenceStringMatch = _referenceStringKeyValueRegex.Match(s);
            if (referenceStringMatch.Success) return EntryType.ReferenceStringKeyValue;

            referenceStringMatch = _referenceStringKeyRegex.Match(ws);
            if (referenceStringMatch.Success) return EntryType.ReferenceStringKey;

            // multiLineMatch
            var multiLineMatch = _multiLineKeyValueRegex.Match(s);
            if (multiLineMatch.Success) return EntryType.MultiLineKeyValue;

            // StringKey
            stringMatch = _stringKeyRegex.Match(s);
            if (stringMatch.Success) return EntryType.StringKey;

            // DigitKey
            digitMatch = _digitKeyRegex.Match(s);
            if (digitMatch.Success) return EntryType.DigitKey;

            throw new ApplicationException($"Unknown entry type: [{s}]");
        }

        #region element parsers

        private StringKeyValue GetBinaryKeyValue(string s)
        {
            var stringMatch = _binaryKeyValueRegex.Match(s);
            // Console.WriteLine("DEBUG: binary key: {0}", stringMatch.Groups[1].Value);

            // keep on reading lines until one ends with a single apostrophe
            while (true)
            {
                // grab the next line
                string line = _reader.ReadLine();
                if (line == null) break;

                if (line.EndsWith(EndBinaryTag) || line.EndsWith(EndBinaryTag2)) break;
            }

            return new StringKeyValue(stringMatch.Groups[1].Value, null);
        }

        private ReferenceStringValue GetDigitKey(string s)
        {
            var value = new ReferenceStringValue();
            var stringMatch = _digitKeyRegex.Match(s);
            value.Add(new StringKeyValue(stringMatch.Groups[1].Value, null));
            return value;
        }

        private StringKeyValue GetDigitKeyValue(string s)
        {
            var digitMatch = _digitKeyValueRegex.Match(s);
            // Console.WriteLine("DEBUG: digit key: {0}, value: {1}", digitMatch.Groups[1].Value, digitMatch.Groups[2].Value);
            return new StringKeyValue(digitMatch.Groups[1].Value, digitMatch.Groups[2].Value);
        }

        private StringKeyValue GetEmptyListKeyValue(string s)
        {
            var stringMatch = _emptyListKeyValueRegex.Match(s);
            // Console.WriteLine("DEBUG: empty list key: {0}", stringMatch.Groups[1].Value);
            return new StringKeyValue(stringMatch.Groups[1].Value, null);
        }

        private StringKeyValue GetEmptyValueKeyValue(string s)
        {
            var stringMatch = _emptyValueKeyValueRegex.Match(s);
            // Console.WriteLine("DEBUG: empty value key: {0}", stringMatch.Groups[1].Value);
            return new StringKeyValue(stringMatch.Groups[1].Value, null);
        }

        private ListObjectKeyValue GetListObjectKeyValue(string s)
        {
            var listObjectMatch = _listObjectKeyValueRegex.Match(s);
            var listObjectKeyValue = new ListObjectKeyValue(listObjectMatch.Groups[1].Value);
            bool foundEof = false;

            while (true)
            {
                // grab the next line
                string line = _reader.ReadLine();

                if (line == null)
                {
                    foundEof = true;
                    break;
                }

                string ws = line.Trim();
                if ((ws == EndBracketsTag) || (ws == EndBracketsTag2)) break;

                // retrieve the object type
                EntryType entryType = GetEntryType(line);
                // Console.WriteLine(entryType);

                switch (entryType)
                {
                    case EntryType.OpenBraces:
                        listObjectKeyValue.Add(GetObjectValue());
                        break;
                    case EntryType.ReferenceStringKey:
                        listObjectKeyValue.Add(GetReferenceStringValue(line));
                        break;
                    case EntryType.StringKey:
                        listObjectKeyValue.Add(GetStringKey(line));
                        break;
                    case EntryType.DigitKey:
                        listObjectKeyValue.Add(GetDigitKey(line));
                        break;
                    default:
                        throw new ApplicationException($"Unhandled entry type encountered: {entryType}");
                }
            }

            if (foundEof)
            {
                throw new ApplicationException("Found an EOF before finding the ending bracket");
            }

            return listObjectKeyValue;
        }

        private StringKeyValue GetMultiLineKeyValue(string s)
        {
            var stringMatch = _multiLineKeyValueRegex.Match(s);

            var sb = new StringBuilder();
            sb.AppendLine(stringMatch.Groups[2].Value);
 
            // keep on reading lines until one ends with a single apostrophe
            while (true)
            {
                // grab the next line
                string line = _reader.ReadLine();
                if (line == null) break;

                string wsLine = line.Trim();
                if ((wsLine == EndStringTag) || (wsLine == EndStringTag2)) break;
                sb.AppendLine(wsLine);
            }

            return new StringKeyValue(stringMatch.Groups[1].Value, sb.ToString());
        }

        private ObjectKeyValue GetObjectKeyValue(string s)
        {
            var objectMatch = _objectKeyValueRegex.Match(s);
            // Console.WriteLine("DEBUG: object key: {0}", objectMatch.Groups[1].Value);
            return new ObjectKeyValue(objectMatch.Groups[1].Value, GetObjectValue());
        }

        private ObjectValue GetObjectValue()
        {
            bool foundEof = false;
            var objectValue = new ObjectValue();

            while (true)
            {
                // grab the next line
                string line = _reader.ReadLine();

                if (line == null)
                {
                    foundEof = true;
                    break;
                }

                // retrieve the object type
                EntryType entryType = GetEntryType(line);
                // Console.WriteLine("--- DEBUG: entryType: {0}", entryType);

                if (entryType == EntryType.EndBraces)
                {
                    var dataTypeMatch = _dataTypeRegex.Match(line);
                    if (dataTypeMatch.Success)
                    {
                        objectValue.DataType = dataTypeMatch.Groups[1].Value;
                        // Console.WriteLine("DEBUG: data type: {0}", dataTypeMatch.Groups[1].Value);
                    }
                    break;
                }

                switch (entryType)
                {
                    case EntryType.ObjectKeyValue:
                        objectValue.Add(GetObjectKeyValue(line));
                        break;
                    case EntryType.ListObjectKeyValue:
                        objectValue.Add(GetListObjectKeyValue(line));
                        break;
                    case EntryType.StringKeyValue:
                        objectValue.Add(GetStringKeyValue(line));
                        break;
                    case EntryType.DigitKeyValue:
                        objectValue.Add(GetDigitKeyValue(line));
                        break;
                    case EntryType.UndefKeyValue:
                        objectValue.Add(GetUndefKeyValue(line));
                        break;
                    case EntryType.ReferenceStringKeyValue:
                        objectValue.Add(GetReferenceKeyValue(line));
                        break;
                    case EntryType.EmptyListKeyValue:
                        objectValue.Add(GetEmptyListKeyValue(line));
                        break;
                    case EntryType.EmptyValueKeyValue:
                        objectValue.Add(GetEmptyValueKeyValue(line));
                        break;
                    case EntryType.BinaryKeyValue:
                        objectValue.Add(GetBinaryKeyValue(line));
                        break;
                    case EntryType.MultiLineKeyValue:
                        objectValue.Add(GetMultiLineKeyValue(line));
                        break;
                    default:
                        throw new ApplicationException(
                            $"Unhandled entry type encountered in GetObjectValue: {entryType}");
                }
            }

            if (foundEof)
            {
                throw new ApplicationException("Found an EOF before finding the ending curly braces for the current object value");
            }

            return objectValue;
        }

        private ReferenceKeyValue GetReferenceKeyValue(string s)
        {
            var stringMatch = _referenceStringKeyValueRegex.Match(s);
            // Console.WriteLine("DEBUG: reference string key: {0}, value: {1}", stringMatch.Groups[1].Value, stringMatch.Groups[2].Value);
            return new ReferenceKeyValue(stringMatch.Groups[1].Value, stringMatch.Groups[2].Value);
        }

        private ReferenceStringValue GetReferenceStringValue(string s)
        {
            var value = new ReferenceStringValue();
            var stringMatch = _referenceStringKeyRegex.Match(s);
            // Console.WriteLine("DEBUG: s: [{0}]", s);
            // Console.WriteLine("DEBUG: reference string key: [{0}]", stringMatch.Groups[1].Value);
            value.Add(new StringKeyValue(stringMatch.Groups[1].Value, null));
            return value;
        }

        private ReferenceStringValue GetStringKey(string s)
        {
            var value = new ReferenceStringValue();
            var stringMatch = _stringKeyRegex.Match(s);
            value.Add(new StringKeyValue(stringMatch.Groups[1].Value, null));
            return value;
        }

        private StringKeyValue GetStringKeyValue(string s)
        {
            var stringMatch = _stringKeyValueRegex.Match(s);
            // Console.WriteLine("DEBUG: string key: {0}, value: {1}", stringMatch.Groups[1].Value, stringMatch.Groups[2].Value);
            return new StringKeyValue(stringMatch.Groups[1].Value, stringMatch.Groups[2].Value);
        }

        private StringKeyValue GetUndefKeyValue(string s)
        {
            var stringMatch = _undefKeyValueRegex.Match(s);
            return new StringKeyValue(stringMatch.Groups[1].Value, null);
        }

        #endregion
    }
}
