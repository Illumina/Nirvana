using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public class CustomAnnotationProvider : ISupplementaryAnnotationProvider
    {
        private string _currentUcscReferenceName;
        private readonly List<string> _customAnnotationDirs;
        private List<ISupplementaryAnnotationReader> _customAnnotationReaders;
        private bool _hasCustomAnnotations;
		
		public GenomeAssembly GenomeAssembly => SupplementaryAnnotationCommon.GetGenomeAssembly(_customAnnotationDirs?.FirstOrDefault());

        public IEnumerable<IDataSourceVersion> DataSourceVersions
            => SupplementaryAnnotationCommon.GetDataSourceVersions(_customAnnotationDirs?.FirstOrDefault());

        /// <summary>
        /// constructor
        /// </summary>
        public CustomAnnotationProvider(IEnumerable<string> dirs)
        {
            _customAnnotationDirs = dirs.ToList();
        }

	    public CustomAnnotationProvider(List<ISupplementaryAnnotationReader> caReaders)
	    {
			if (caReaders==null) return;
		    _hasCustomAnnotations = true;
		    _customAnnotationReaders = caReaders;
	    }

	    public void AddAnnotation(IVariantFeature variant)
        {
            if (!_hasCustomAnnotations) return;
            variant.AddCustomAnnotation(_customAnnotationReaders);
        }

        public void Load(string ucscReferenceName, IChromosomeRenamer renamer)
        {
			if (_customAnnotationDirs == null || _customAnnotationDirs.Count == 0 || ucscReferenceName == _currentUcscReferenceName) return;

            _customAnnotationReaders = SupplementaryAnnotationCommon.GetReaders(_customAnnotationDirs, ucscReferenceName);
            _hasCustomAnnotations = _customAnnotationReaders.Count > 0;
            _currentUcscReferenceName = ucscReferenceName;
        }

        public void Clear()
        {
            _hasCustomAnnotations = false;
        }
    }
}
