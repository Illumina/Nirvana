using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine.Utilities;
using ErrorHandling.Exceptions;
using Jasix.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;

namespace Jasix
{
    public class OnTheFlyIndexCreator : IDisposable
    {
        private readonly Stream _indexStream;
        private readonly JasixIndex _jasixIndex;
        private int _lastPosition;
        private string _lastChromName;

        #region IDisposable
        bool _disposed;

        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
                _jasixIndex.Write(_indexStream);
                _indexStream.Flush();
                _indexStream.Dispose();
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
            // Free any other managed objects here.

        }
        #endregion

        public OnTheFlyIndexCreator(Stream indexStream)
        {
            _indexStream = indexStream;
            _jasixIndex  = new JasixIndex();
        }

        public void SetHeader(string header)
        {
            _jasixIndex.HeaderLine = header;
        }

        public void Add(IPosition position, long fileLocation)
        {
            var chromName = position.VcfFields[VcfCommon.ChromIndex];//we want to preserve the chrom name from input
            var start     = position.Start;
            var end       = position.InfoData.End;

            if (chromName == _lastChromName && start < _lastPosition)
            {
                throw new UserErrorException($"the Json file is not sorted at {position.Chromosome.UcscName}: {start}");
            }
            _lastPosition  = start;
            _lastChromName = chromName;

            if (end == null)
            {
                var altAlleles = position.AltAlleles;
                int altAlleleOffset = altAlleles != null && altAlleles.All(Utilities.IsNucleotideAllele) && altAlleles.Any(x => x.Length > 1) ? 1 : 0;

                end = Math.Max(position.RefAllele.Length - 1, altAlleleOffset) + start;
            }

            _jasixIndex.Add(position.Chromosome.EnsemblName, start, end.Value, fileLocation);
        }

    }
}