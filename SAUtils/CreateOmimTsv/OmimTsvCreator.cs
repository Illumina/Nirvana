using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VariantAnnotation.GeneAnnotation;
using System.Linq;
using SAUtils.TsvWriters;
using SAUtils.InputFileParsers;
using VariantAnnotation.Providers;

namespace SAUtils.CreateOmimTsv
{
    public class OmimTsvCreator
    {
        private Stream _geneMap2FileStream;
        private Stream _mim2GenefileStream;
        private GeneSymbolUpdater _geneSymbolUpdater;
        private SortedList<string, List<OmimEntry>> _gene2Mims;
        private string _outputDirectory;

        public OmimTsvCreator(Stream geneMap2FileStream, Stream mim2GenefileStream, GeneSymbolUpdater geneSymbolUpdater, string outputDirectory)
        {
            _geneMap2FileStream = geneMap2FileStream;
            _mim2GenefileStream = mim2GenefileStream;
            _geneSymbolUpdater = geneSymbolUpdater;
            _outputDirectory = outputDirectory;
            _gene2Mims = new SortedList<string, List<OmimEntry>> { };
        }

        public void Create()
        {
            var geneMap2Reader = new OmimReader(_geneMap2FileStream, _geneSymbolUpdater);
            var mimToGeneReader = new OmimReader(_mim2GenefileStream, _geneSymbolUpdater);

            foreach(var entry in geneMap2Reader)
            {
                if (!_gene2Mims.ContainsKey(entry.Hgnc))
                    _gene2Mims[entry.Hgnc] = new List<OmimEntry>();

                _gene2Mims[entry.Hgnc].Add(entry);
            }

            foreach(var entry in mimToGeneReader)
            {
                if (!_gene2Mims.ContainsKey(entry.Hgnc))
                {
                    _gene2Mims[entry.Hgnc] = new List<OmimEntry> { entry};
                    continue;
                }
                if (_gene2Mims[entry.Hgnc].Select(x => x.MimNumber).Contains(entry.MimNumber)) continue;

                throw new Exception($"{entry.Hgnc} exist in geneMap2.txt but {entry.MimNumber} not exist");
            }

            var dataSourceVersion = GetSourceVersion("");
            var outPutPath = "";
            using (var omimWriter = new GeneAnnotationTsvWriter(outPutPath, dataSourceVersion, null, 0, "omim", true))
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
