﻿using System.Collections.Generic;
using System.Text;

namespace JSON;

public sealed class JsonObject
{
    private readonly StringBuilder _sb;
    private          bool          _needsComma;
    private          int           _nestedLevel;

    public const  char   Comma        = ',';
    private const char   DoubleQuote  = '\"';
    public const  char   OpenBracket  = '[';
    public const  char   CloseBracket = ']';
    public const  char   OpenBrace    = '{';
    public const  char   CloseBrace   = '}';
    private const string ColonString  = "\":";

    public JsonObject(StringBuilder sb) => _sb = sb;

    private void AddKey(string description)
    {
        _sb.Append(DoubleQuote);
        _sb.Append(description);
        _sb.Append(ColonString);
    }

    public bool AddBoolValue(string description, bool b, bool outputFalse = false)
    {
        // we do not want to print out false flags by default.
        if (!b && !outputFalse) return false;

        if (_needsComma) _sb.Append(Comma);
        AddKey(description);

        _sb.Append(b ? "true" : "false");
        _needsComma = true;

        return true;
    }

    public bool AddIntValue(string description, int? i)
    {
        if (i == null) return false;

        if (_needsComma) _sb.Append(Comma);
        AddKey(description);

        _sb.Append(i);
        _needsComma = true;

        return true;
    }

    public bool AddDoubleValue(string description, double? d, string format = "0.####")
    {
        if (d == null) return false;

        if (_needsComma) _sb.Append(Comma);
        AddKey(description);
        _sb.Append(d.Value.ToString(format));
        _needsComma = true;

        return true;
    }

    public bool AddStringValue(string description, string s, bool useQuote = true)
    {
        if (string.IsNullOrEmpty(s) || s == ".") return false;

        if (_needsComma) _sb.Append(Comma);
        AddKey(description);

        if (useQuote) _sb.Append(DoubleQuote);
        _sb.Append(s);
        if (useQuote) _sb.Append(DoubleQuote);
        _needsComma = true;

        return true;
    }

    public bool AddStringValues(string description, IEnumerable<string> values, bool useQuote = true)
    {
        if (values == null) return false;

        var validEntries = new List<string>();
        foreach (string value in values) if (value != ".") validEntries.Add(value);

        if (validEntries.Count == 0) return false;

        if (_needsComma) _sb.Append(Comma);
        AddKey(description);
        _sb.Append(OpenBracket);

        var needsComma = false;

        foreach (string value in validEntries)
        {
            if (needsComma) _sb.Append(Comma);
            if (useQuote) _sb.Append(DoubleQuote);
            _sb.Append(value);
            if (useQuote) _sb.Append(DoubleQuote);
            needsComma = true;
        }

        _sb.Append(CloseBracket);
        _needsComma = true;

        return true;
    }

    public bool AddObjectValues<T>(string description, IEnumerable<T> values) where T : IJsonSerializer
    {
        if (values == null) return false;

        if (_needsComma) _sb.Append(Comma);
        AddKey(description);
        _sb.Append(OpenBracket);

        var needsComma = false;

        foreach (var value in values)
        {
            // comma handling
            if (needsComma) _sb.Append(Comma);
            else needsComma = true;
            value.SerializeJson(_sb);
        }
            
        _sb.Append(CloseBracket);
        _needsComma = true;

        return true;
    }
}