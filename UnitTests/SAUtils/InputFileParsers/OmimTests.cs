using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.InputFileParsers.OMIM.SAUtils.CreateOmimTsv;
using SAUtils.Omim;
using Xunit;
using SAUtils.InputFileParsers.OMIM;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class OmimTests
    {
        private Stream GetGeneMap2Stream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("# Copyright (c) 1966-2018 Johns Hopkins University. Use of this file adheres to the terms specified at https://omim.org/help/agreement.");
            writer.WriteLine("# Generated: 2018-02-13");
            writer.WriteLine("# See end of file for additional documentation on specific fields");
            writer.WriteLine("# Chromosome\tGenomic Position Start\tGenomic Position End\tCyto Location\tComputed Cyto Location\tMim Number\tGene Symbols\tGene Name\tApproved Symbol\tEntrez Gene ID\tEnsembl Gene ID\tComments\tPhenotypes\tMouse Gene Symbol/ID");
            writer.WriteLine("chr1\t0\t27600000\t1p36\t\t605462\tBCC1\tBasal cell carcinoma, susceptibility to, 1\t\t100307118\t\tassociated with rs7538876\t{Basal cell carcinoma, susceptibility to, 1}, 605462 (2)\t");
            writer.WriteLine("chr1\t0\t27600000\t1p36\t\t155600\tCMM, MLM, DNS\tCutaneous malignant melanoma/dysplastic nevus\tCMM\t1243\t\tsome linkage studies negative; see 9p\t{Melanoma, cutaneous malignant, 1}, 155600 (2), Autosomal dominant\t");
            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        private GeneSymbolUpdater GetGeneSymbolUpdater()
        {
            var entrezGeneIdToSymbol = new Dictionary<string, string>
            {
                { "entrez123", "ALPQTL2"},
                { "entrez234", "AD7CNTP" },
                { "entrez345", "BCC1"},
                { "entrez456", "CMM, MLM, DNS"}
            };
            var ensemblGeneIdToSymbol = new Dictionary<string, string>
            {
                { "ensembl123", "ALPQTL2"},
                { "ensembl234", "AD7CNTP" },
                { "ensembl345", "BCC1"},
                { "ensembl456", "CMM, MLM, DNS"}
            };

            return new GeneSymbolUpdater(entrezGeneIdToSymbol, ensemblGeneIdToSymbol);
        }

        [Fact]
        public void GetItems()
        {
            using (var reader = new OmimParser(new StreamReader(GetGeneMap2Stream()), GetGeneSymbolUpdater(), OmimSchema.Get()))
            {
                var items = reader.GetItems().ToList();

                Assert.Single(items);
                Assert.Equal("{\"mimNumber\":155600,\"description\":\"Cutaneous malignant melanoma/dysplastic nevus\",\"phenotypes\":[{\"mimNumber\":155600,\"phenotype\":\"Melanoma, cutaneous malignant, 1\",\"mapping\":\"disease phenotype itself was mapped\",\"inheritances\":[\"Autosomal dominant\"],\"comments\":\"contribute to susceptibility to multifactorial disorders or to susceptibility to infection\"}]}", items[0].GetJsonString());
            }

        }

    }
}