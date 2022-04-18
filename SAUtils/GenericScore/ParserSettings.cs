using System;
using System.Collections.Generic;

namespace SAUtils.GenericScore
{
    public sealed class ParserSettings
    {
        public readonly ColumnIndex ColumnIndex;
        public readonly string[]    PossibleAlleles;
        public          bool        IsPositional => ColumnIndex.AltAllele == null;

        public readonly Action<string, double, Dictionary<string, double>> ConflictResolutionFunction;

        public ParserSettings(
            ColumnIndex columnIndex,
            string[] possibleAlleles,
            Action<string, double, Dictionary<string, double>> conflictResolutionFunction
        )
        {
            ColumnIndex                = columnIndex;
            PossibleAlleles            = possibleAlleles;
            ConflictResolutionFunction = conflictResolutionFunction;
        }
    }

    public sealed class ColumnIndex
    {
        public readonly ushort  Chromosome;
        public readonly ushort  Position;
        public readonly ushort? RefAllele;
        public readonly ushort? AltAllele;
        public readonly ushort  Score;
        public readonly ushort  Others;

        public ColumnIndex(
            ushort chromosome,
            ushort position,
            ushort? refAllele,
            ushort? altAllele,
            ushort score,
            ushort? others
        )
        {
            Chromosome = chromosome;
            Position   = position;
            RefAllele  = refAllele;
            AltAllele  = altAllele;
            Score      = score;
            Others     = others ?? ushort.MaxValue;
        }
    }
}