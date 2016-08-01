using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public class ClinVarItem : SupplementaryDataItem, IClinVar, IJsonSerializer
    {
        #region members

        public string AlleleOrigin { get; private set; }
        public string AltAllele { get; internal set; }
        public string SaAltAllele { get; internal set; }
        private int AlleleIndex { get; }
        public string ReviewStatusString { get; private set; }
        public string GeneReviewsID { get; private set; }
        public string ID { get; internal set; }
        public ReviewStatus ReviewStatus { get; private set; }
        public string IsAlleleSpecific { get; set; }
        public string MedGenID { get; private set; }
        public string OmimID { get; private set; }
        public string OrphanetID { get; internal set; }
        public string DiseaseDbNames { get; private set; }
        public string DiseaseDbIds { get; private set; }
        public string Phenotype { get; private set; }
        public string ReferenceAllele { get; private set; }
        public string Significance { get; internal set; }
        public string SnoMedCtID { get; private set; }
        internal HashSet<long> PubMedIds;
        public IEnumerable<long> PubmedIds => PubMedIds;
        public long LastEvaluatedDate { get; internal set; }

        private readonly int _hashCode;
        private readonly string _infoField;

        public static int InconsistantClinvarItemCount;

        private static readonly Dictionary<string, ReviewStatus> ReviewStatusNameMapping = new Dictionary<string, ReviewStatus>()
        {
            ["no_assertion"] = ReviewStatus.no_assertion,
            ["no_criteria"] = ReviewStatus.no_criteria,
            ["guideline"] = ReviewStatus.practice_guideline,
            ["single"] = ReviewStatus.single_submitter,
            ["mult"] = ReviewStatus.multiple_submitters,
            ["conf"] = ReviewStatus.conflicting_interpretations,
            ["exp"] = ReviewStatus.expert_panel
        };


        #endregion
        #region Equality Overrides

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object o)
        {
            // If parameter cannot be cast to ClinVarItem return false:
            var other = o as ClinVarItem;
            if ((object)other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        public static bool operator ==(ClinVarItem a, ClinVarItem b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return (a.Start == b.Start) &&
                   (a.Chromosome == b.Chromosome) &&
                   (a.ID == b.ID) &&
                   (a.AlleleOrigin == b.AlleleOrigin) &&
                   (a.AltAllele == b.AltAllele) &&
                   (a.GeneReviewsID == b.GeneReviewsID) &&
                   (a.MedGenID == b.MedGenID) &&
                   (a.OmimID == b.OmimID) &&
                   (a.OrphanetID == b.OrphanetID) &&
                   (a.Phenotype == b.Phenotype) &&
                   (a.ReferenceAllele == b.ReferenceAllele) &&
                   (a.Significance == b.Significance) &&
                   (a.SnoMedCtID == b.SnoMedCtID);
        }

        public static bool operator !=(ClinVarItem a, ClinVarItem b)
        {
            return !(a == b);
        }

        #endregion

        public ClinVarItem(string chromosome,
            int position,
            string refAllele,
            string alternateAllele,
            int alleleIndex,
            string infoFields)
        {
            Chromosome = chromosome;
            Start = position;
            ReferenceAllele = refAllele;
            AltAllele = alternateAllele;
            AlleleIndex = alleleIndex;
            _infoField = infoFields;

            ParseInfoFields();//set available clinvar fields from infofields
        }



        public ClinVarItem(string chromosome,
            int position,
            string alleleOrigin,
            string altAllele,
            string geneReviewsId,
            string id,
            string reviewStatusString,
            string medGenId,
            string omimId,
            string orphanetId,
            string phenotype,
            string referenceAllele,
            string significance,
            string snoMedCtId,
            HashSet<long> pubmedIds = null,
            long lastEvaluatedDate = long.MinValue
            )
        {
            Chromosome = chromosome;
            Start = position;
            AlleleOrigin = alleleOrigin != null && alleleOrigin.All(char.IsNumber) ? InterpretAlleleOrigin(Convert.ToInt32(alleleOrigin)) : alleleOrigin;
            AltAllele = altAllele;
            SaAltAllele = altAllele;
            GeneReviewsID = geneReviewsId;
            ID = id;
            ReviewStatusString = reviewStatusString;
            MedGenID = medGenId;
            OmimID = omimId;
            OrphanetID = orphanetId;
            Phenotype = phenotype;
            ReferenceAllele = referenceAllele;
            Significance = significance != null && significance.All(char.IsNumber) ? InterpretSignificance(Convert.ToInt32(significance)) : significance;
            SnoMedCtID = snoMedCtId;
            PubMedIds = pubmedIds;
            LastEvaluatedDate = lastEvaluatedDate;
            IsAlleleSpecific = null;

            if (ReviewStatusNameMapping.ContainsKey(reviewStatusString))
                ReviewStatus = ReviewStatusNameMapping[reviewStatusString];

            _hashCode = CalculateHashCode();
        }

        public ClinVarItem(ExtendedBinaryReader reader)
        {
            Read(reader);
        }

        private void ParseInfoFields()
        {
            // 1       883516  rs267598747     G       A       .       .       RS=267598747;RSPOS=883516;dbSNPBuildID=137;SSR=0;SAO=3;VP=0x050060000305000002100120;GENEINFO=NOC2L:26155;WGT=1;VC=SNV;PM;REF;SYN;ASP;LSD;
            // CLNALLE=1;CLNHGVS=NC_000001.10:g.883516G>A;CLNSRC=ClinVar;CLNORIGIN=2;CLNSRCID=NM_015658.3:c.1654C>T;CLNSIG=255;CLNDSDB=MedGen:SNOMED_CT;CLNDSDBID=C0025202:2092003;CLNDBN=Malignant_melanoma;CLNREVSTAT=not;CLNACC=RCV000064926.2
            // 1       2160305 rs387907306     G       A,T     .       .      RS=387907306;RSPOS=2160305;dbSNPBuildID=137;SSR=0;SAO=0;VP=0x050060000a05000002110100;GENEINFO=SKI:6497;WGT=1;VC=SNV;PM;NSM;REF;ASP;LSD;OM;CLNALLE=1,2;CLNHGVS=NC_000001.10:g.2160305G>A,NC_000001.10:g.2160305G>T;CLNSRC=ClinVar|OMIM_Allelic_Variant,ClinVar|OMIM_Allelic_Variant;CLNORIGIN=1,1;CLNSRCID=NM_003036.3:c.100G>A|164780.0004,NM_003036.3:c.100G>T|164780.0005;CLNSIG=5,5;CLNDSDB=GeneReviews:MedGen:OMIM:Orphanet:SNOMED_CT,GeneReviews:MedGen:OMIM:Orphanet:SNOMED_CT;CLNDSDBID=NBK1277:C1321551:182212:ORPHA2462:83092002,NBK1277:C1321551:182212:ORPHA2462:83092002;CLNDBN=Shprintzen-Goldberg_syndrome,Shprintzen-Goldberg_syndrome;CLNREVSTAT=single,single;CLNACC=RCV000030819.24,RCV000030820.24


            var infoFields = _infoField.Split(';');

            foreach (var infoField in infoFields)
            {
                if (!infoField.Contains("="))
                    continue;
                var key = infoField.Split('=')[0];
                var value = infoField.Split('=')[1];

                switch (key)
                {
                    case "CLNACC":
                        ID = value.Split(',')[AlleleIndex];
                        break;
                    case "CLNORIGIN":
                        AlleleOrigin = value.All(char.IsNumber) ? InterpretAlleleOrigin(Convert.ToInt32(value)) : value.Split(',')[AlleleIndex];
                        break;
                    case "CLNSIG":
                        Significance = value.All(char.IsNumber) ? InterpretSignificance(Convert.ToInt32(value)) : value.Split(',')[AlleleIndex];
                        break;
                    case "CLNDBN":
                        Phenotype = value.Split(',')[AlleleIndex];
                        break;
                    case "CLNDSDB":
                        DiseaseDbNames = value.Split(',')[AlleleIndex];
                        break;
                    case "CLNREVSTAT":
                        ReviewStatusString = value.Split(',')[AlleleIndex];
                        break;
                    case "CLNDSDBID":
                        DiseaseDbIds = value.Split(',')[AlleleIndex];
                        break;
                }
            }
        }

        public void SetDiseaseDbIds(string diseaseDbId, string diseaseDbName)
        {
            var diseaseDbIds = diseaseDbId.Split(':');
            var diseaseDbNames = diseaseDbName.Split(':');

            if (diseaseDbIds.Length != diseaseDbNames.Length) //this should not happen
            {
                // skipping such entries
                InconsistantClinvarItemCount++;
                return;
            }

            for (int i = 0; i < diseaseDbIds.Length; i++)
            {
                switch (diseaseDbNames[i])
                {
                    case "MedGen":
                        MedGenID = AddDiseaseDbId(MedGenID, diseaseDbIds[i]);
                        break;
                    case "SNOMED_CT":
                        SnoMedCtID = AddDiseaseDbId(SnoMedCtID, diseaseDbIds[i]);
                        break;
                    case "OMIM":
                        OmimID = AddDiseaseDbId(OmimID, diseaseDbIds[i]);
                        break;
                    case "Orphanet":
                        OrphanetID = AddDiseaseDbId(OrphanetID, diseaseDbIds[i]);
                        break;
                    case "GeneReviews":
                        GeneReviewsID = AddDiseaseDbId(GeneReviewsID, diseaseDbIds[i]);
                        break;
                }
            }
        }

        private static string AddDiseaseDbId(string oldId, string id)
        {
            if (string.IsNullOrEmpty(oldId)) return id;

            return oldId + "," + id;
        }

        private static string InterpretSignificance(int value)
        {
            //##INFO=<ID=CLNSIG,Number=.,Type=String,Description="Variant Clinical Significance, 0 - Uncertain significance, 1 - not provided, 2 - Benign, 3 - Likely benign, 4 - Likely pathogenic, 5 - Pathogenic, 6 - drug response, 7 - histocompatibility, 255 - other">
            switch (value)
            {
                case 0:
                    return "uncertain significance";
                case 1:
                    return "not provided";
                case 2:
                    return "benign";
                case 3:
                    return "likely benign";
                case 4:
                    return "likely pathogenic";
                case 5:
                    return "pathogenic";
                case 6:
                    return "drug response";
                case 7:
                    return "histocompatibility";
                case 255:
                    return "other";
                default:
                    return null;
            }
        }

        private static string InterpretAlleleOrigin(int value)
        {
            /*##INFO=<ID=CLNORIGIN,Number=.,Type=String,Description="Allele Origin. One or more of the following values may be added: 0 - unknown; 1 - germline; 2 - somatic; 4 - inherited; 8 - paternal; 16 - maternal; 32 - de-novo; 64 - biparental; 128 - uniparental; 256 - not-tested; 512 - tested-inconclusive; 1073741824 - other">
             */
            var interpretation = new StringBuilder();

            if (value == 0)
                return "unknown";
            if (value == 1073741824)
                return "other";
            if ((value & 1) != 0)
                interpretation.Append("germline,");
            if ((value & 2) != 0)
                interpretation.Append("somatic,");
            if ((value & 4) != 0)
                interpretation.Append("inherited,");
            if ((value & 8) != 0)
                interpretation.Append("paternal,");
            if ((value & 16) != 0)
                interpretation.Append("maternal,");
            if ((value & 32) != 0)
                interpretation.Append("de-novo,");
            if ((value & 64) != 0)
                interpretation.Append("biparental,");
            if ((value & 128) != 0)
                interpretation.Append("uniparental,");
            if ((value & 256) != 0)
                interpretation.Append("not-tested,");
            if ((value & 512) != 0)
                interpretation.Append("tested-inconclusive,");

            return interpretation.Length == 0 ? null : interpretation.ToString(0, interpretation.Length - 1);
        }

        /// <summary>
        /// calculates the hash code for this object
        /// </summary>
        // ReSharper disable once FunctionComplexityOverflow
        private int CalculateHashCode()
        {
            int hashCode = Start.GetHashCode();
            if (Chromosome != null) hashCode ^= Chromosome.GetHashCode();
            if (ID != null) hashCode ^= ID.GetHashCode();
            if (AlleleOrigin != null) hashCode ^= AlleleOrigin.GetHashCode();
            if (AltAllele != null) hashCode ^= AltAllele.GetHashCode();
            if (GeneReviewsID != null) hashCode ^= GeneReviewsID.GetHashCode();
            if (MedGenID != null) hashCode ^= MedGenID.GetHashCode();
            if (OmimID != null) hashCode ^= OmimID.GetHashCode();
            if (OrphanetID != null) hashCode ^= OrphanetID.GetHashCode();
            if (Phenotype != null) hashCode ^= Phenotype.GetHashCode();
            if (ReferenceAllele != null) hashCode ^= ReferenceAllele.GetHashCode();
            if (Significance != null) hashCode ^= Significance.GetHashCode();
            if (SnoMedCtID != null) hashCode ^= SnoMedCtID.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Adds the ClinVar items in this object to the supplementary annotation object
        /// </summary>
        public override SupplementaryDataItem SetSupplementaryAnnotations(SupplementaryAnnotation sa, string refBases = null)
        {
            // check if the ref allele matches the refBases as a prefix
            if (!SupplementaryAnnotation.ValidateRefAllele(ReferenceAllele, refBases))
            {
                return null; //the ref allele for this entry did not match the reference bases.
            }

            // for insertions and deletions, the alternate allele has to be modified to conform with VEP convension
            int newStart = Start;
            var newAlleles = SupplementaryAnnotation.GetReducedAlleles(ReferenceAllele, AltAllele, ref newStart);

            var newRefAllele = newAlleles.Item1;
            var newAltAllele = newAlleles.Item2;

            if (newRefAllele != ReferenceAllele)
            {
                var additionalItem = new ClinVarItem(Chromosome, newStart, AlleleOrigin, newAltAllele, GeneReviewsID, ID, ReviewStatusString, MedGenID, OmimID, OrphanetID, Phenotype, newRefAllele, Significance, SnoMedCtID, PubMedIds, LastEvaluatedDate);

                return additionalItem;
            }


            sa.ClinVarItems.Add(this);

            return null;
        }

        public override SupplementaryInterval GetSupplementaryInterval()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// returns a string representation of this object
        /// </summary>
        public override string ToString()
        {
            return string.Join("\t",
                Chromosome,
                Start,
                ID,
                ReviewStatusString,
                ReferenceAllele,
                AltAllele,
                AlleleOrigin,
                GeneReviewsID,
                MedGenID,
                OmimID,
                OrphanetID,
                Phenotype,
                Significance,
                SnoMedCtID);
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteAsciiString(AlleleOrigin);
            writer.WriteAsciiString(SaAltAllele);
            writer.WriteAsciiString(ReferenceAllele);
            writer.WriteAsciiString(GeneReviewsID);
            writer.WriteAsciiString(ID);
            writer.WriteByte((byte)ReviewStatus);
            writer.WriteAsciiString(IsAlleleSpecific);
            writer.WriteAsciiString(MedGenID);
            writer.WriteAsciiString(OmimID);
            writer.WriteAsciiString(OrphanetID);

            writer.WriteUtf8String(SupplementaryAnnotation.ConvertMixedFormatString(Phenotype));
            writer.WriteAsciiString(Significance);
            writer.WriteAsciiString(SnoMedCtID);

            writer.WriteLong(LastEvaluatedDate);

            if (PubmedIds == null)
                writer.WriteInt(0);
            else
            {
                writer.WriteInt(PubmedIds.Count());
                foreach (var pubmedId in PubmedIds)
                {
                    writer.WriteLong(pubmedId);
                }
            }

        }

        private void Read(ExtendedBinaryReader reader)
        {
            AlleleOrigin = reader.ReadAsciiString();
            SaAltAllele = reader.ReadAsciiString();
            AltAllele = SaAltAllele != null ? SupplementaryAnnotation.ReverseSaReducedAllele(SaAltAllele) : null; // A
            ReferenceAllele = reader.ReadAsciiString();
            GeneReviewsID = reader.ReadAsciiString();
            ID = reader.ReadAsciiString();
            ReviewStatus = (ReviewStatus)reader.ReadByte();
            IsAlleleSpecific = reader.ReadAsciiString();
            MedGenID = reader.ReadAsciiString();
            OmimID = reader.ReadAsciiString();
            OrphanetID = reader.ReadAsciiString();
            Phenotype = reader.ReadUtf8String();
            Significance = reader.ReadAsciiString();
            SnoMedCtID = reader.ReadAsciiString();
            LastEvaluatedDate = reader.ReadLong();

            var count = reader.ReadInt();//no of pubmed ids
            if (count == 0)
            {
                PubMedIds = null;
                return;
            }
            var pubmedSet = new HashSet<long>();
            for (int i = 0; i < count; i++)
                pubmedSet.Add(reader.ReadLong());

            PubMedIds = pubmedSet;
        }
        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);

            //converting empty alleles to '-'
            if (string.IsNullOrEmpty(ReferenceAllele)) ReferenceAllele = "-";
            if (string.IsNullOrEmpty(AltAllele)) AltAllele = "-";

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("id", ID);
            jsonObject.AddStringValue("reviewStatus", ReviewStatus.ToString().Replace("_", " "));
            jsonObject.AddStringValue("isAlleleSpecific", IsAlleleSpecific, false);
            jsonObject.AddStringValue("alleleOrigin", AlleleOrigin);
            jsonObject.AddStringValue("refAllele", "N" == ReferenceAllele ? null : ReferenceAllele);
            jsonObject.AddStringValue("altAllele", "N" == AltAllele ? null : AltAllele);
            jsonObject.AddStringValue("phenotype", Phenotype?.Replace('_', ' '));
            jsonObject.AddStringValue("geneReviewsId", GeneReviewsID);
            jsonObject.AddStringValue("medGenId", MedGenID);
            jsonObject.AddStringValue("omimId", OmimID);
            jsonObject.AddStringValue("orphanetId", OrphanetID);
            jsonObject.AddStringValue("significance", Significance);
            jsonObject.AddStringValue("snoMedCtId", SnoMedCtID);
            if (LastEvaluatedDate != long.MinValue)
                jsonObject.AddStringValue("lastEvaluatedDate", new DateTime(LastEvaluatedDate).ToString("yyyy-MM-dd"));
            if (PubmedIds != null)
                jsonObject.AddStringValues("pubMedIds", PubmedIds.Select(id => id.ToString()));
            sb.Append(JsonObject.CloseBrace);
        }

    }
}
