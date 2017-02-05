using System.Collections.Generic;
using System.Text;
using VariantAnnotation.Algorithms.Consequences;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures
{
    public sealed class GeneFusionAnnotation : IJsonSerializer
    {
        #region members

        private readonly int? _exon;
        private readonly int? _intron;
        private readonly List<GeneFusion> _fusions;
        private readonly BreakendTranscriptAnnotation _pos1Annotation;

        #endregion

        private sealed class GeneFusion : IJsonSerializer
        {
            public List<ConsequenceType> Consequences;
            public string HgvsCodingName;
            public int? Exon;
            public int? Intron;

            public void SerializeJson(StringBuilder sb)
            {
                var jsonObject = new JsonObject(sb);

                // data section
                sb.Append(JsonObject.OpenBrace);

                jsonObject.AddStringValue("hgvsc", HgvsCodingName);

                var consequence = new Consequences(Consequences, null);

                if (Consequences.Count > 0) jsonObject.AddStringValues("consequence", consequence.GetConsequenceStrings());

                jsonObject.AddIntValue("intron", Intron);
                jsonObject.AddIntValue("exon", Exon);

                sb.Append(JsonObject.CloseBrace);
            }
        }

        public GeneFusionAnnotation(BreakendTranscriptAnnotation breakendTa)
        {
            _exon = breakendTa.Exon;
            _intron = breakendTa.Intron;
            _fusions = new List<GeneFusion>();
            _pos1Annotation = breakendTa;
        }

        public void AddGeneFusion(BreakendTranscriptAnnotation pos2Annotation)
        {
            if (!_pos1Annotation.IsGeneFusion(pos2Annotation)) return;
            var fusion = new GeneFusion
            {
                Exon = pos2Annotation.Exon,
                Intron = pos2Annotation.Intron,
                HgvsCodingName = AssignHgvsc(pos2Annotation),
                Consequences = new List<ConsequenceType>()
            };

            if (IsUnidirectionalGeneFusion(pos2Annotation)) fusion.Consequences.Add(ConsequenceType.UnidirectionalGeneFusion);
            if (IsBidirectionalGeneFusion(pos2Annotation)) fusion.Consequences.Add(ConsequenceType.BidirectionalGeneFusion);
            _fusions.Add(fusion);

        }

        private string AssignHgvsc(BreakendTranscriptAnnotation pos2Annotation)
        {
            var hgvsFusionNameBuilder = new StringBuilder(_pos1Annotation.HgvsDescription);
            if (_pos1Annotation.ConsistentOrientation == pos2Annotation.ConsistentOrientation)
            {
                hgvsFusionNameBuilder.Append("_o" + pos2Annotation.HgvsDescription);
            }
            else
            {
                hgvsFusionNameBuilder.Append("_" + pos2Annotation.HgvsDescription);
            }
            return hgvsFusionNameBuilder.ToString();
        }

        private bool IsUnidirectionalGeneFusion(BreakendTranscriptAnnotation pos2Annotation)
        {
            return _pos1Annotation.ConsistentOrientation != pos2Annotation.ConsistentOrientation;
        }

        private bool IsBidirectionalGeneFusion(BreakendTranscriptAnnotation pos2Annotation)
        {
            return _pos1Annotation.ConsistentOrientation == pos2Annotation.ConsistentOrientation;
        }

        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);

            // data section
            sb.Append(JsonObject.OpenBrace);

            jsonObject.AddIntValue("intron", _intron);
            jsonObject.AddIntValue("exon", _exon);

            if (_fusions.Count > 0) jsonObject.AddObjectValues("fusions", _fusions);

            sb.Append(JsonObject.CloseBrace);
        }
    }
}