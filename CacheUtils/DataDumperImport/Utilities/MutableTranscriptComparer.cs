using System;
using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;

namespace CacheUtils.DataDumperImport.Utilities
{
    internal sealed class MutableTranscriptComparer : EqualityComparer<MutableTranscript>
    {
        private static bool GeneEquals(MutableGene x, MutableGene y)
        {
            return x.Chromosome.Index == y.Chromosome.Index &&
                   x.Start            == y.Start            &&
                   x.End              == y.End              &&
                   x.OnReverseStrand  == y.OnReverseStrand  &&
                   x.GeneId           == y.GeneId           &&
                   x.Symbol           == y.Symbol           &&
                   x.HgncId           == y.HgncId           &&
                   x.SymbolSource     == y.SymbolSource;
        }

        private static bool ExonEquals(MutableExon x, MutableExon y)
        {
            return x.Start == y.Start &&
                   x.End   == y.End   &&
                   x.Phase == y.Phase;
        }

        private static bool IntervalEquals(IInterval x, IInterval y)
        {
            return x.Start == y.Start &&
                   x.End   == y.End;
        }

        private static bool CdnaMapEquals(ICdnaCoordinateMap x, ICdnaCoordinateMap y)
        {
            return x.Start     == y.Start     &&
                   x.End       == y.End       &&
                   x.CdnaStart == y.CdnaStart &&
                   x.CdnaEnd   == y.CdnaEnd;
        }

        // ReSharper disable SuggestBaseTypeForParameter
        private static bool ArrayEquals<T>(T[] x, T[] y, Func<T, T, bool> equals)
        // ReSharper restore SuggestBaseTypeForParameter
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            if (x.Length != y.Length)   return false;
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int i = 0; i < x.Length; i++) if (!equals(x[i], y[i])) return false;
            return true;
        }

        private static bool IntEquals(int x, int y) => x == y;

        public override bool Equals(MutableTranscript x, MutableTranscript y)
        {
                return x.Chromosome.Index      == y.Chromosome.Index                                &&
                       x.Start                 == y.Start                                           &&
                       x.End                   == y.End                                             &&
                       x.Id                    == y.Id                                              &&
                       x.Version               == y.Version                                         &&
                       x.CcdsId                == y.CcdsId                                          &&
                       x.RefSeqId              == y.RefSeqId                                        &&
                       x.Source                == y.Source                                          &&
                       x.TotalExonLength       == y.TotalExonLength                                 &&
                       x.TranslateableSequence == y.TranslateableSequence                           &&
                       x.CdsStartNotFound      == y.CdsStartNotFound                                &&
                       x.CdsEndNotFound        == y.CdsEndNotFound                                  &&
                       x.StartExonPhase        == y.StartExonPhase                                  &&
                       x.BioType               == y.BioType                                         &&
                       x.IsCanonical           == y.IsCanonical                                     &&
                       x.ProteinId             == y.ProteinId                                       &&
                       x.ProteinVersion        == y.ProteinVersion                                  &&
                       x.PeptideSequence       == y.PeptideSequence                                 &&
                       x.SiftData              == y.SiftData                                        &&
                       x.PolyphenData          == y.PolyphenData                                    &&
                       GeneEquals(x.Gene, y.Gene)                                                   &&
                       ArrayEquals(x.Exons, y.Exons, ExonEquals)                                    &&
                       ArrayEquals(x.Introns, y.Introns, IntervalEquals)                            &&
                       ArrayEquals(x.MicroRnas, y.MicroRnas, IntervalEquals)                        &&
                       ArrayEquals(x.SelenocysteinePositions, y.SelenocysteinePositions, IntEquals) &&
                       ArrayEquals(x.CdnaMaps, y.CdnaMaps, CdnaMapEquals)                           &&
                       CdnaMapEquals(x.CodingRegion, y.CodingRegion);
        }

        public override int GetHashCode(MutableTranscript x)
        {
            unchecked
            {
                var hashCode = x.Chromosome.Index.GetHashCode();
                hashCode = (hashCode * 397) ^ x.Start.GetHashCode();
                hashCode = (hashCode * 397) ^ x.End.GetHashCode();
                hashCode = (hashCode * 397) ^ x.Id.GetHashCode();
                hashCode = (hashCode * 397) ^ x.Version.GetHashCode();
                hashCode = (hashCode * 397) ^ x.BioType.GetHashCode();
                hashCode = (hashCode * 397) ^ x.Source.GetHashCode();
                return hashCode;
            }
        }
    }
}
