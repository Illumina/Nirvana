using System.Globalization;
using OptimizedCore;

namespace SAUtils.ParseUtils;

public sealed class SplitLine
{
    private readonly string[] _splitLine;

    public SplitLine(in string inputLine, in char delimiter)
    {
        _splitLine = inputLine.OptimizedSplit(delimiter);
    }

    public string GetString(in int index)
    {
        return _splitLine[index];
    }

    public int? ParseInteger(in int index)
    {
        return ParseInteger(_splitLine[index]);
    }

    public double? ParseDouble(in int index)
    {
        return ParseDouble(_splitLine[index]);
    }

    public static int? ParseInteger(string valueString)
    {
        return int.TryParse(
            valueString,
            NumberStyles.Integer | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out int temp
        )
            ? temp
            : null;
    }

    public static double? ParseDouble(string valueString)
    {
        return double.TryParse(valueString, out double temp) ? temp : null;
    }
}