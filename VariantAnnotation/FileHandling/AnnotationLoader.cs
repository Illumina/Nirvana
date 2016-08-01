using System;
using System.Collections.Generic;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.CytogeneticBands;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling
{
    // A delegate type for hooking up change notifications.
    public delegate void ChangedEventHandler(object sender, EventArgs e);

    public sealed class AnnotationLoader
    {
        #region members

        public string CurrentReferenceName { get; private set; }
        public GenomeAssembly GenomeAssembly { get; private set; }
        public ChromosomeRenamer ChromosomeRenamer { get; }

        private readonly HashSet<string> _missingRefFiles;
        private CompressedSequenceReader _compressedSequenceReader;
        public ICompressedSequence CompressedSequence;
        public ICytogeneticBands CytogeneticBands;
        private readonly PerformanceMetrics _performanceMetrics;

        public event ChangedEventHandler Changed;

        #endregion

        /// <summary>
        /// private constructor for our singleton
        /// </summary>
        private AnnotationLoader()
        {
            CompressedSequence  = new CompressedSequence();
            ChromosomeRenamer   = new ChromosomeRenamer();
            _performanceMetrics = PerformanceMetrics.Instance;
            _missingRefFiles    = new HashSet<string>();
        }

        /// <summary>
        /// access AnnotationLoader.Instance to get the singleton object
        /// </summary>
        public static AnnotationLoader Instance { get; } = new AnnotationLoader();

	    public void Clear()
	    {
		    CurrentReferenceName = "";
	    }

	    /// <summary>
        /// loads the compressed sequence file
        /// </summary>
        public void LoadCompressedSequence(string inputCompressedReferencePath)
        {
            Instance._compressedSequenceReader = new CompressedSequenceReader(inputCompressedReferencePath);
            LoadReaderData();
        }

        /// <summary>
        /// loads the cytogenetic bands and the genome assembly
        /// </summary>
        private void LoadReaderData()
        {
            CytogeneticBands = Instance._compressedSequenceReader.CytogeneticBands;
            GenomeAssembly   = Instance._compressedSequenceReader.GenomeAssembly;
        }

        /// <summary>
        /// Loads new annotations if the current reference sequence changes
        /// </summary>
	    public bool Load(string ucscReferenceName)
        {
            if (_missingRefFiles.Contains(ucscReferenceName)) return false;
            if (ucscReferenceName == CurrentReferenceName) return true;

            CurrentReferenceName = ucscReferenceName;

            try
            {
                ReadSequence();
                OnChanged(EventArgs.Empty);
                return true;
            }
            catch (GeneralException)
            {
                _missingRefFiles.Add(CurrentReferenceName);
                Console.WriteLine("The cache file was not found for this reference sequence (disabling annotation).");
                return false;
            }
        }

	    /// <summary>
        /// loads the compressed sequence for this particular reference sequence
        /// </summary>
        private void ReadSequence()
        {
            _performanceMetrics.StartReference(CurrentReferenceName);
            var ensemblRefSeq = ChromosomeRenamer.GetEnsemblReferenceName(CurrentReferenceName);
            _compressedSequenceReader.GetCompressedSequence(ensemblRefSeq, ref CompressedSequence);
            _performanceMetrics.StopReference();
        }

        /// <summary>
        /// Invoke the Changed event; called whenever list changes
        /// </summary>
        private void OnChanged(EventArgs e)
        {
            Changed?.Invoke(this, e);
        }
    }
}
