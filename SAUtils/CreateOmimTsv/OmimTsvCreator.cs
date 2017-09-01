using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VariantAnnotation.GeneAnnotation;
using System.Linq;
using Compression.Utilities;
using SAUtils.TsvWriters;
using SAUtils.InputFileParsers;
using VariantAnnotation.Providers;

namespace SAUtils.CreateOmimTsv
{
    public class OmimTsvCreator
    {
        private const string _jsonKeyName = "omim";
        private FileInfo _geneMap2File;
        private FileInfo _mim2Genefile;
        private GeneSymbolUpdater _geneSymbolUpdater;
        private SortedList<string, List<OmimEntry>> _gene2Mims;
        private string _outputDirectory;

        public OmimTsvCreator(FileInfo geneMap2File, FileInfo mim2Genefile,  GeneSymbolUpdater geneSymbolUpdater, string outputDirectory)
        {
            _geneMap2File = geneMap2File;
            _mim2Genefile = mim2Genefile;

            _geneSymbolUpdater = geneSymbolUpdater;
            _outputDirectory = outputDirectory;
            _gene2Mims = new SortedList<string, List<OmimEntry>> { };
        }

        public void Create()
        {
            var geneMap2Reader = new OmimReader(GZipUtilities.GetAppropriateReadStream(_geneMap2File.FullName), _geneSymbolUpdater);
            var mimToGeneReader = _mim2Genefile != null? new OmimReader(GZipUtilities.GetAppropriateReadStream(_mim2Genefile.FullName), _geneSymbolUpdater):null;

            foreach(var entry in geneMap2Reader)
            {
                if (!_gene2Mims.ContainsKey(entry.Hgnc))
                    _gene2Mims[entry.Hgnc] = new List<OmimEntry>();

                _gene2Mims[entry.Hgnc].Add(entry);
            }

            if (mimToGeneReader != null)
            {
                foreach (var entry in mimToGeneReader)
                {
                    if (!_gene2Mims.ContainsKey(entry.Hgnc))
                    {
                        _gene2Mims[entry.Hgnc] = new List<OmimEntry> { entry };
                        continue;
                    }
                    if (_gene2Mims[entry.Hgnc].Select(x => x.MimNumber).Contains(entry.MimNumber)) continue;

                    throw new Exception($"{entry.Hgnc} exist in geneMap2.txt but {entry.MimNumber} not exist");
                }

            }
            

            var dataSourceVersion = GetSourceVersion(_geneMap2File.FullName);

            using (var omimWriter = new GeneAnnotationTsvWriter(_outputDirectory, dataSourceVersion, null, 0, _jsonKeyName, true))
            {
                foreach (var kvp in _gene2Mims)
                {           
                    omimWriter.AddEntry(kvp.Key, kvp.Value.Select(x=>x.ToString()).ToList());
                }
            }

        }


        private static DataSourceVersion GetSourceVersion(string dataFileName)
        {
            var versionFileName = dataFileName + ".version";

            if (!File.Exists(versionFileName))
            {
                throw new FileNotFoundException(versionFileName);
            }

            var versionReader = new DataSourceVersionReader(versionFileName);
            var version = versionReader.GetVersion();
            Console.WriteLine(version.ToString());
            return version;
        }
    }
}
