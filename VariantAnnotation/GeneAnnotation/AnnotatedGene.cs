using System.Text;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;

namespace VariantAnnotation.GeneAnnotation
{
    public sealed class AnnotatedGene:IAnnotatedGene
    {
        private const string NullGene = "nullGene";
        public string GeneName { get; }
        public IGeneAnnotationSource[] Annotations { get; }

        public AnnotatedGene(string geneName, IGeneAnnotationSource[] annotations)
        {
            GeneName = geneName;
            Annotations = annotations;
        }

        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);
            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("name",GeneName);
            foreach (var geneAnnotation in Annotations)
            {
                if (geneAnnotation.IsArray)
                {
                    jsonObject.AddStringValues(geneAnnotation.DataSource, geneAnnotation.JsonStrings,false);
                }
                else
                {
                    jsonObject.AddStringValue(geneAnnotation.DataSource, geneAnnotation.JsonStrings[0],false);
                }
                
            }
            
            sb.Append(JsonObject.CloseBrace);
        }

        public void Write(IExtendedBinaryWriter writer)
        {
            writer.Write(GeneName);
            writer.WriteOpt(Annotations.Length);
            for (int i = 0; i < Annotations.Length; i++)
                Annotations[i].Write(writer);
        }

        public static IAnnotatedGene Read(IExtendedBinaryReader reader)
        {
            var geneName = reader.ReadAsciiString();
            var annotationLength = reader.ReadOptInt32();
            var annotations = new IGeneAnnotationSource[annotationLength];
            for (int i = 0; i < annotationLength; i++)
            {
                annotations[i] = GeneAnnotationSource.Read(reader);
            }

            return geneName == NullGene ? null : new AnnotatedGene(geneName, annotations);
        }

        public int CompareTo(IAnnotatedGene other)
        {
            return GeneName.CompareTo(other.GeneName);
        }

        public static IAnnotatedGene CreateEmptyGene()
        {
            return new AnnotatedGene(NullGene, new IGeneAnnotationSource[0]);
        }
    }
}