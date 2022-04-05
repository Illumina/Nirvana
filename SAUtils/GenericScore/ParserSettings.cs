using System;
using System.Collections.Generic;

namespace SAUtils.GenericScore
{
    public sealed class ParserSettings
    {
        public readonly ColumnPositions                                     ColumnPositions;
        public readonly string[]                                            PossibleAlleles;
        public readonly Action<string, double, Dictionary<string, double>> ConflictResolutionFunction;

        public ParserSettings(
            ColumnPositions columnPositions,
            string[] possibleAlleles, Action<string, double, Dictionary<string, double>> conflictResolutionFunction)
        {
            ColumnPositions            = columnPositions;
            PossibleAlleles            = possibleAlleles;
            ConflictResolutionFunction = conflictResolutionFunction;
        }
    }

    public sealed class ColumnPositions
    {
        public readonly ushort Chromosome;
        public readonly ushort Position;
        public readonly ushort RefAllele;
        public readonly ushort AltAllele;
        public readonly ushort Score;
        public readonly ushort Others;

        public ColumnPositions(
            ushort chromosome,
            ushort position,
            ushort? refAllele,
            ushort? altAllele,
            ushort? score,
            ushort? others
        )
        {
            Chromosome = chromosome;
            Position   = position;
            RefAllele  = refAllele ?? ushort.MaxValue;
            AltAllele  = altAllele ?? ushort.MaxValue;
            Score      = score     ?? ushort.MaxValue;
            Others     = others    ?? ushort.MaxValue;
        }
    }
}