using System;
using System.IO;
using CommonUtilities;
using Compression.FileHandling;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;

namespace SAUtils.TsvWriters
{
    public sealed class SaMiscTsvWriter:IDisposable
    {

        private const int ReferenceWindow = 10;
        private readonly BgzipTextWriter _bgzipTextWriter;
        private readonly TsvIndex _tsvIndex;
        private string _currentChromosome;
        private readonly ISequenceProvider _sequenceProvider;

        #region IDisposable

        private bool _disposed;

        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// protected implementation of Dispose pattern. 
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                _bgzipTextWriter.Dispose();
                _tsvIndex.Dispose();
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
            // Free any other managed objects here.

        }
        #endregion


        public SaMiscTsvWriter(string outputPath,DataSourceVersion dataSourceVersion, string assembly,string keyName, ISequenceProvider sequenceProvider)
        {
            var fileName = keyName + "_" + dataSourceVersion.Version.Replace(" ", "_") + ".misc.tsv.gz";
            _bgzipTextWriter = new BgzipTextWriter(Path.Combine(outputPath, fileName));

            _bgzipTextWriter.Write(GetHeader(dataSourceVersion,assembly));

            _tsvIndex = new TsvIndex(Path.Combine(outputPath, fileName + ".tvi"));
            _sequenceProvider = sequenceProvider;

        }

        private static string GetHeader(DataSourceVersion dataSourceVersion, string assembly)
        {
            var sb = StringBuilderCache.Acquire();

            sb.Append($"#name={dataSourceVersion.Name}\n");
            sb.Append($"#assembly={assembly}\n");
            sb.Append($"#version={dataSourceVersion.Version}\n");
            sb.Append($"#description={dataSourceVersion.Description}\n");
            sb.Append("#CHROM\tPOS\tGlobalMajorAllele\n");
            return StringBuilderCache.GetStringAndRelease(sb);
        }


        public void AddEntry(string chromosome, int position, string globalMajorAllele,string refAllele)
        {
            if (globalMajorAllele == null)
            {
                throw new Exception($"no global major allele for {chromosome}:{position}");
            }

            //validate the vcf reference
            if (!ValidateReference(chromosome, position, refAllele)) return;

            if (!chromosome.Equals(_currentChromosome))
            {
                _tsvIndex.AddTagPosition(chromosome, _bgzipTextWriter.Position);
                //Console.WriteLine($"chr {chromosome}, filePos: {_bgzipTextWriter.Position}");
                _currentChromosome = chromosome;
            }

            _bgzipTextWriter.Write($"{chromosome}\t{position}\t{globalMajorAllele}\n");

        }


        private bool ValidateReference(string chromosome, int position, string refAllele)
        {
            if (_sequenceProvider == null) return true;

            var refDictionary = _sequenceProvider.RefNameToChromosome;
            if (!refDictionary.ContainsKey(chromosome)) return false;

            var chrom = refDictionary[chromosome];

            _sequenceProvider.LoadChromosome(chrom);
            var refSequence = _sequenceProvider.Sequence.Substring(position - 1, ReferenceWindow);
            return SupplementaryAnnotationUtilities.ValidateRefAllele(refAllele, refSequence);
        }

    }
}