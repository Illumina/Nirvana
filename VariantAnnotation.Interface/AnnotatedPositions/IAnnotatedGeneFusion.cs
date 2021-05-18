﻿// ReSharper disable InconsistentNaming

using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IAnnotatedGeneFusion : IJsonSerializer
    {
        ITranscript transcript  { get; }
        int?        exon        { get; }
        int?        intron      { get; }
        string      hgvsr       { get; }
        bool        isInFrame   { get; }
        ulong       geneKey     { get; }
        string[]    geneSymbols { get; }
    }
}