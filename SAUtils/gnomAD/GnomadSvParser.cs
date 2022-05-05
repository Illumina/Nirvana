using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;
using SAUtils.ParseUtils;

namespace SAUtils.gnomAD;

public abstract class GnomadSvParser : IDisposable
{
    private const      char                             CommentChar = '#';
    private readonly   StreamReader                     _reader;
    protected readonly Dictionary<string, Chromosome> RefNameDict;

    protected readonly char       Delimiter = '\t';
    protected          TsvIndices TsvIndices;

    protected GnomadSvParser(
        StreamReader reader,
        Dictionary<string, Chromosome> refNameDict
    )
    {
        _reader     = reader;
        RefNameDict = refNameDict;
    }

    public IEnumerable<GnomadSvItem> GetItems()
    {
        string line;
        while ((line = _reader.ReadLine()) != null)
        {
            // Skip empty lines and comment lines
            if (string.IsNullOrWhiteSpace(line) || line.OptimizedStartsWith(CommentChar))
                continue;

            GnomadSvItem gnomadSvItem = ParseLine(line);
            if (gnomadSvItem == null)
                continue;

            yield return gnomadSvItem;
        }
    }

    protected abstract GnomadSvItem ParseLine(string inputLine);

    public void Dispose()
    {
        _reader?.Dispose();
    }
}