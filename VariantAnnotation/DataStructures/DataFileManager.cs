using System;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.Variants;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures
{
    public sealed class DataFileManager
    {
        public delegate void ChangedEventHandler(object sender, NewReferenceEventArgs e);

        public event ChangedEventHandler Changed;

        private readonly CompressedSequenceReader _compressedSequenceReader;
        private ushort _currentReferenceIndex = ushort.MaxValue;
        private readonly ICompressedSequence _compressedSequence;

        /// <summary>
        /// constructor
        /// </summary>
        public DataFileManager(CompressedSequenceReader reader, ICompressedSequence compressedSequence)
        {
            _compressedSequence = compressedSequence;
            _compressedSequenceReader = reader;
        }

        public void AssignCytogeneticBand(VariantFeature variant)
        {
            var cytogeneticBands = _compressedSequence.CytogeneticBands;
            if (cytogeneticBands == null) return;

            variant.CytogeneticBand = cytogeneticBands.GetCytogeneticBand(variant.ReferenceIndex,
                variant.VcfReferenceBegin, variant.VcfReferenceEnd);
        }

        public void LoadReference(ushort refIndex, Action clearDataSources, PerformanceMetrics metrics = null)
        {
            if (refIndex == _currentReferenceIndex) return;

            if (refIndex == ChromosomeRenamer.UnknownReferenceIndex)
            {
                clearDataSources();
                return;
            }
            
            var referenceData = new ReferenceNameData
            {
                ReferenceIndex       = refIndex,
                UcscReferenceName    = _compressedSequence.Renamer.UcscReferenceNames[refIndex],
                EnsemblReferenceName = _compressedSequence.Renamer.EnsemblReferenceNames[refIndex]
            };

            metrics?.StartReference(referenceData.UcscReferenceName);
            _compressedSequenceReader.GetCompressedSequence(referenceData.EnsemblReferenceName);
            metrics?.StopReference();
            _currentReferenceIndex = refIndex;

            Changed?.Invoke(this, new NewReferenceEventArgs(referenceData));
        }
    }
}
