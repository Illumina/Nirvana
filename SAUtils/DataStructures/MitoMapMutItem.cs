using System;
using System.Text;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.Sequence;

namespace SAUtils.DataStructures
{
    public sealed class MitoMapMutItem : SupplementaryDataItem
    {      
        private readonly string _disease;
        private bool? _homoplasmy;
        private bool? _heteroplasmy;
        private readonly string _status;
        private readonly string _clinicalSignificance;
        private readonly string _scorePercentile;

        public MitoMapMutItem(int posi, string refAllele, string altAllele, string disease, bool? homoplasmy, bool? heteroplasmy, string status, string clinicalSignificance, string scorePercentile)
        {
            Chromosome = new Chromosome("chrM", "MT", 24);
            Start = posi;
            ReferenceAllele = refAllele;
            AlternateAllele = altAllele;
            IsInterval = false;
            _disease = disease;
            _homoplasmy = homoplasmy;
            _heteroplasmy = heteroplasmy;
            _status = status;
            _clinicalSignificance = clinicalSignificance;
            _scorePercentile = scorePercentile;
        }

        public string GetJsonString()
        {
            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("refAllele", ReferenceAllele);
            jsonObject.AddStringValue("altAllele", AlternateAllele);
            if (!string.IsNullOrEmpty(_disease)) jsonObject.AddStringValue("disease", _disease);
            if (_homoplasmy.HasValue) jsonObject.AddStringValue("hasHomoplasmy", _homoplasmy.ToString());
            if (_heteroplasmy.HasValue) jsonObject.AddStringValue("hasHeteroplasmy", _heteroplasmy.ToString());
            if (!string.IsNullOrEmpty(_status)) jsonObject.AddStringValue("status", _status);
            if (!string.IsNullOrEmpty(_clinicalSignificance)) jsonObject.AddStringValue("clinicalSignificance", _clinicalSignificance);
            if(!string.IsNullOrEmpty(_scorePercentile)) jsonObject.AddStringValue("scorePercentile", _scorePercentile);

            return sb.ToString();
        }

        public override SupplementaryIntervalItem GetSupplementaryInterval()
        {
            throw new System.NotImplementedException();
        }
    }
}
