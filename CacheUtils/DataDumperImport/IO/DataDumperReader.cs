using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.FauxRegex;
using IO;
using OptimizedCore;

namespace CacheUtils.DataDumperImport.IO
{
    public sealed class DataDumperReader : IDisposable
    {
        private readonly StreamReader _reader;
        private readonly StringBuilder _sb = new StringBuilder();

        public DataDumperReader(Stream stream) => _reader = FileUtilities.GetStreamReader(stream);

        private string GetNextLine() => _reader.ReadLine();

        public ObjectKeyValueNode GetRootNode()
        {
            string line = GetNextLine();
            if (line == null) throw new InvalidDataException("Expected a root object node, but no data was found.");

            var results = RegexDecisionTree.GetEntryType(line);
            if (results.Type != EntryType.RootObjectKeyValue) throw new InvalidDataException($"Expected a root object node, but found a {results.Type} node.");

            return new ObjectKeyValueNode(results.Key, GetObjectValue());
        }

        private static StringValueNode GetDigitKey(string key) => new StringValueNode(key);

        private ListObjectKeyValueNode GetListObjectKeyValue(string key)
        {
            var listObjectKeyValue = new ListObjectKeyValueNode(key);

            while (true)
            {
                string line = GetNextLine().Trim().TrimEnd(',');
                if (line == "]") break;

                var results = RegexDecisionTree.GetEntryType(line);

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (results.Type)
                {
                    case EntryType.OpenBraces:
                        listObjectKeyValue.Add(GetObjectValue());
                        break;
                    case EntryType.DigitKey:
                        listObjectKeyValue.Add(GetDigitKey(line));
                        break;
                    default:
                        throw new InvalidDataException($"Unhandled entry type encountered: {results.Type}");
                }
            }

            return listObjectKeyValue;
        }

        private StringKeyValueNode GetMultiLineKeyValue(string key, string value)
        {
            _sb.Clear();
            _sb.Append(value);

            while (true)
            {
                string line = GetNextLine().Trim();
                if (line.OptimizedStartsWith('\'')) break;
                _sb.Append(' ');
                _sb.Append(line);
            }

            return new StringKeyValueNode(key, _sb.ToString());
        }

        private ObjectValueNode GetObjectValue()
        {
            var type = "(unknown)";
            var nodes   = new List<IImportNode>();

            while (true)
            {
                string line = GetNextLine();
                var results = RegexDecisionTree.GetEntryType(line);

                if (results.Type == EntryType.EndBraces || results.Type == EntryType.EndBracesWithDataType)
                {
                    if (results.Type == EntryType.EndBracesWithDataType) type = results.Key;
                    break;
                }

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (results.Type)
                {
                    case EntryType.ObjectKeyValue:
                        nodes.Add(new ObjectKeyValueNode(results.Key, GetObjectValue()));
                        break;
                    case EntryType.ListObjectKeyValue:
                        nodes.Add(GetListObjectKeyValue(results.Key));
                        break;
                    case EntryType.DigitKeyValue:
                    case EntryType.StringKeyValue:
                    case EntryType.ReferenceStringKeyValue:
                        nodes.Add(new StringKeyValueNode(results.Key, results.Value));
                        break;
                    case EntryType.UndefKeyValue:
                    case EntryType.EmptyListKeyValue:
                    case EntryType.EmptyValueKeyValue:
                        nodes.Add(new StringKeyValueNode(results.Key, null));
                        break;
                    case EntryType.MultiLineKeyValue:
                        nodes.Add(GetMultiLineKeyValue(results.Key, results.Value));
                        break;
                    default:
                        throw new InvalidDataException($"Unhandled entry type encountered in GetObjectValue: {results.Type}: [{line}]");
                }
            }

            return new ObjectValueNode(type, nodes);
        }

        public void Dispose() => _reader.Dispose();
    }
}
