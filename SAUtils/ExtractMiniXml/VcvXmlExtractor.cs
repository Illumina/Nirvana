using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Compression.Utilities;
using IO;

namespace SAUtils.ExtractMiniXml
{
    public class VcvXmlExtractor
    {
        private readonly string _inputXmlFile;
        private readonly string _outputDir;
        private readonly List<string> _vcvIds;
        
        private const string VcvRecordTag = "VariationArchive";

        private const string XmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n" +
                                         "<ClinVarVariationRelease xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"http://ftp.ncbi.nlm.nih.gov/pub/clinvar/xsd_public/clinvar_variation/variation_archive_1.4.xsd\" ReleaseDate=\"2019-12-31\">\n";

        private const string XmlFooter = "\n</ClinVarVariationRelease>";
        
        public VcvXmlExtractor(string inputXmlFile, List<string> vcvIds, string outputDir)
        {
            _inputXmlFile = inputXmlFile;
            _vcvIds       = vcvIds;
            _outputDir    = outputDir;
        }

        public void Extract()
        {
            using (var reader = GZipUtilities.GetAppropriateStreamReader(_inputXmlFile))
            using (var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, IgnoreWhitespace = true }))
            {
                var existVarSet = xmlReader.ReadToDescendant(VcvRecordTag);

                while (_vcvIds.Count > 0 && existVarSet)
                {
                    var contents = xmlReader.ReadOuterXml();

                    var rcv = DetectVcv(_vcvIds, contents);
                    
                    if (rcv !=null)
                    {
                        var targetedContent =contents;
                        var outXmlFile      = Path.Combine(_outputDir, rcv + ".xml");
                        WriteToFile(outXmlFile, targetedContent);
                    }
                    if(!xmlReader.IsStartElement(VcvRecordTag))
                        existVarSet = xmlReader.ReadToNextSibling(VcvRecordTag);
                }

            }

            if (_vcvIds.Count > 0)
            {
                Console.WriteLine($"Failed to Find {string.Join(',',_vcvIds)}");
            }

        }
        
        private static void WriteToFile(string fileName, string targetedContent)
        {
            using (var writer = new StreamWriter(FileUtilities.GetCreateStream(fileName)))
            {
                writer.Write(XmlHeader);
                writer.Write(targetedContent);
                writer.Write(XmlFooter);
                Console.WriteLine($"Creating/ updating {fileName}");
            }
        }

        private static string DetectVcv(List<string> vcvs, string rcvContents)
        {
            foreach (var vcv in vcvs)
            {
                if (rcvContents.Contains(vcv))
                {
                    vcvs.Remove(vcv);
                    return vcv;
                }
            }

            return null;
        }
        

    }
}