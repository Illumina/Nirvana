using System;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.AnnotationSources;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;

namespace VariantAnnotation.FileHandling.JSON
{
	public sealed class UnifiedJsonWriter: IDisposable
	{
		#region members

        private readonly StreamWriter _writer;

        #endregion

        #region IDisposable

        private bool _isDisposed;

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
            lock (this)
            {
                if (_isDisposed) return;

                if (disposing)
                {
                    // Free any other managed objects here.
                    WriteFooter();
                    _writer.Dispose();
                }

                // Free any unmanaged objects here. 
                _isDisposed = true;
            }
        }

        #endregion

		public UnifiedJsonWriter(string outputPath, string creationTime, string vepDataVersion, IEnumerable<IDataSourceVersion> iDataSourceVersions, string[] sampleNames)
		{
			// open the vcf file
			_writer = GZipUtilities.GetStreamWriter(outputPath);
			_writer.NewLine = "\n";

			var dataSourceVersions = iDataSourceVersions as List<DataSourceVersion>;

			// write the header
			_writer.Write(UnifiedJson.GetHeader(NirvanaAnnotationSource.GetVersion(), creationTime,
				JsonCommon.SchemaVersion, vepDataVersion, dataSourceVersions, sampleNames));
		}

		/// <summary>
		/// write the footer
		/// </summary>
		private void WriteFooter()
        {
            _writer.WriteLine();
            _writer.WriteLine(JsonCommon.Footer);
        }

        /// <summary>
        /// writes the variant to the current output stream
        /// </summary>
        public void Write(string variantString)
        {
            _writer.Write(variantString);
        }
	}
}
