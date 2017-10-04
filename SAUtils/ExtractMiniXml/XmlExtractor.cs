using System;
using System.IO;
using System.Xml;
using Compression.Utilities;
using VariantAnnotation.Utilities;

namespace SAUtils.ExtractMiniXml
{
	public sealed class XmlExtractor
	{
		private readonly string _inputXmlFile;
		private readonly string _outXmlFile;
		private readonly string _rcvId;

		private const string XmlHeader = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>"+"\n"+ "<ReleaseSet Dated=\"2016-07-04\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" Type=\"full\" xsi:noNamespaceSchemaLocation=\"http://ftp.ncbi.nlm.nih.gov/pub/clinvar/xsd_public/clinvar_public_1.35.xsd\">"+"\n\n";

		private const string XmlFooter = "\n\n</ReleaseSet>";

		public XmlExtractor(string inputXmlFile, string rcvId, string outputDir)
		{
			_inputXmlFile = inputXmlFile;
			_rcvId = rcvId;
			_outXmlFile = outputDir == null ? rcvId + ".xml" : Path.Combine(outputDir, rcvId + ".xml");
		}

		public void Extract()
		{
			var findRcv = false;
			string targetedContent =null;

			using (var reader = GZipUtilities.GetAppropriateStreamReader(_inputXmlFile))
			using (var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, IgnoreWhitespace = true }))
			{
				var existVarSet = xmlReader.ReadToDescendant("ClinVarSet");

				while (!findRcv && existVarSet)
				{
					 var rcvContents = xmlReader.ReadOuterXml();

					findRcv = rcvContents.Contains(_rcvId);
					if (findRcv)
						targetedContent = rcvContents;
					if(!xmlReader.IsStartElement("ClinVarSet"))
						existVarSet = xmlReader.ReadToNextSibling("ClinVarSet");
				}
			

			}

			if (!findRcv)
			{
				Console.WriteLine($"Failed to Find {_rcvId}");
				return ;
			}

			using (var writer = new StreamWriter(FileUtilities.GetCreateStream(_outXmlFile)))
			{
				writer.Write(XmlHeader);
				writer.Write(targetedContent);
				writer.Write(XmlFooter);
				Console.WriteLine($"Find {_rcvId}, output in {_outXmlFile}");
			}

		}
	}
}