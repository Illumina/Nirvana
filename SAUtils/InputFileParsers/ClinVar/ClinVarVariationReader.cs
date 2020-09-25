using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using IO;

namespace SAUtils.InputFileParsers.ClinVar
{
    public sealed class ClinVarVariationReader : IDisposable
    {
        private const string VcvRecordTag      = "VariationArchive";
        private const string AccessionTag      = "Accession";
        private const string VersionTag        = "Version";
        private const string DateTag           = "DateLastUpdated";
        private const string ReviewStatusTag   = "ReviewStatus";
        private const string InterpretedRecordTag = "InterpretedRecord";
        private const string InterpretationsTag   = "Interpretations";
        private const string InterpretationTag    = "Interpretation";

        private const string IncludedRecordTag = "IncludedRecord";
        
        private const string DescriptionTag = "Description";
        private const string ExplanationTag = "Explanation";
        private const string TypeTag        = "Type";


        private readonly Stream _readStream;

        public ClinVarVariationReader(Stream readStream)
        {
            _readStream = readStream;
        }

        public IEnumerable<VcvItem> GetItems()
        {
            using (var reader = FileUtilities.GetStreamReader(_readStream))
            using (var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, IgnoreWhitespace = true}))
            {
                xmlReader.ReadToDescendant(VcvRecordTag);
                do
                {
                    var  subTreeReader = xmlReader.ReadSubtree();
                    var xElement       = XElement.Load(subTreeReader);
                    
                    var item = ExtractVariantRecord(xElement);
                    
                    if (item == null) continue;
                    yield return item;

                } while (xmlReader.ReadToNextSibling(VcvRecordTag));
            }
        }

        private static VcvItem ExtractVariantRecord(XElement xElement)
        {
            if (xElement == null || xElement.IsEmpty) return null;
            
            var accession  = xElement.Attribute(AccessionTag)?.Value;
            var version    = xElement.Attribute(VersionTag)?.Value;
            var dateString       = xElement.Attribute(DateTag)?.Value;
            var date        = ClinVarReader.ParseDate(dateString);

            var interpretationRecord = xElement.Element(InterpretedRecordTag);
            var includedRecord = xElement.Element(IncludedRecordTag);
            
            //expecting one of the two to be non-null
            if (!((interpretationRecord == null || interpretationRecord.IsEmpty) ^
                  (includedRecord       == null || includedRecord.IsEmpty)))
            {
                throw new DataMisalignedException("Only one of interpretation/included records should be present for "+ accession);
            }
            
            if (interpretationRecord != null && !interpretationRecord.IsEmpty)
            {
                var interpretedSignificances = GetSignificances(interpretationRecord.Element(InterpretationsTag));

                var interpretedReviewStatusString = interpretationRecord.Element(ReviewStatusTag)?.Value;
                if(interpretedReviewStatusString ==null) throw new MissingFieldException($"No review status provided for {accession}.{version}");
            
                var interpretedReviewStatus = ClinVarCommon.ReviewStatusNameMapping[interpretedReviewStatusString];
                return new VcvItem(accession, version, date, interpretedReviewStatus, interpretedSignificances);
            }
            
            var includedSignificances = GetSignificances(includedRecord.Element(InterpretationsTag));

            var includedReviewStatusString = includedRecord.Element(ReviewStatusTag)?.Value;
            if(includedReviewStatusString ==null) throw new MissingFieldException($"No review status provided for {accession}.{version}");
            
            var reviewStatus = ClinVarCommon.ReviewStatusNameMapping[includedReviewStatusString];
            return new VcvItem(accession, version, date, reviewStatus, includedSignificances);
        }

        
        private static List<string> GetSignificances(XElement interpretations)
        {
            if (interpretations == null || interpretations.IsEmpty) return null;
            
            var significanceList = new List<string>();
            foreach (var interpretation in interpretations.Elements(InterpretationTag))
            {
                var type = interpretation.Attribute(TypeTag)?.Value;
                if(type==null || type != "Clinical significance") continue;
                
                var description = interpretation.Element(DescriptionTag)?.Value.ToLower();
                var explanation = interpretation.Element(ExplanationTag)?.Value.ToLower();
                if(description == null && explanation == null) continue;

                var significances = ClinVarCommon.GetSignificances(description, explanation);
                foreach (var significance in significances)
                {
                    if (!ClinVarCommon.ValidPathogenicity.Contains(significance)) 
                        throw new InvalidDataException($"Invalid clinical significance found. Observed: {significance}");
                    significanceList.Add(significance);
                }
            }
            return significanceList;
        }

        public void Dispose()
        {
            _readStream?.Dispose();
        }
    }
}