using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Compression.Utilities;
using IO;

namespace SAUtils.ExtractMiniXml
{
	public sealed class XmlExtractor
	{
		private readonly string _inputXmlFile;
		private readonly string _outputDir;
		private readonly string _rcvIds;

		private const string XmlHeader = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>"+"\n"+ "<ReleaseSet Dated=\"2016-07-04\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" Type=\"full\" xsi:noNamespaceSchemaLocation=\"http://ftp.ncbi.nlm.nih.gov/pub/clinvar/xsd_public/clinvar_public_1.35.xsd\">"+"\n\n";

		private const string XmlFooter = "\n\n</ReleaseSet>";

		public XmlExtractor(string inputXmlFile, string rcvIds, string outputDir)
		{
			_inputXmlFile = inputXmlFile;
			_rcvIds       = rcvIds;
            _outputDir    = outputDir;
        }

		public void Extract()
		{
            var rcvs = ExtractRcvIds(_rcvIds);


			using (var reader = GZipUtilities.GetAppropriateStreamReader(_inputXmlFile))
			using (var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, IgnoreWhitespace = true }))
			{
				var existVarSet = xmlReader.ReadToDescendant("ClinVarSet");

                while (rcvs.Count > 0 && existVarSet)
				{
					 var rcvContents = xmlReader.ReadOuterXml();

					var rcv = DetectRcv(rcvs, rcvContents);
                    
                    if (rcv!=null)
                    {
                        var targetedContent =rcvContents;
                        var outXmlFile = Path.Combine(_outputDir, rcv + ".xml");
                        WriteToFile(outXmlFile, targetedContent);
                    }
					if(!xmlReader.IsStartElement("ClinVarSet"))
						existVarSet = xmlReader.ReadToNextSibling("ClinVarSet");
				}

			}

			if (rcvs.Count > 0)
			{
				Console.WriteLine($"Failed to Find {string.Join(',',rcvs)}");
			}

		}

        private static string DetectRcv(List<string> rcvs, string rcvContents)
        {
            foreach (var rcv in rcvs)
            {
                if (rcvContents.Contains(rcv))
                {
                    rcvs.Remove(rcv);
                    return rcv;
                }
            }

            return null;
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

        private static List<string> ExtractRcvIds(string rcvIds)
        {
            var ids= new List<string>();
            if (Directory.Exists(rcvIds))
            {
                foreach (var fileName in Directory.EnumerateFiles(rcvIds))
                {
                    if(fileName.Contains("RCV")) ids.Add(Path.GetFileNameWithoutExtension(fileName));
                }

                return ids;
            }

            return rcvIds.Split(',').ToList();
        }
    }
}