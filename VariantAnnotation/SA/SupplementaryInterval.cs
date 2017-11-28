using System;
using System.IO;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.SA
{
    public sealed class SupplementaryInterval : ISupplementaryInterval
    {
        public string KeyName { get; }
        public string ReferenceName { get; }
        public int Start { get; }
        public int End { get; }
        public string JsonString { get; }
        public ReportFor ReportingFor { get; }

        public SupplementaryInterval(string keyName, string refName, int start, int end, string jsonString,
            ReportFor reportingFor)
        {
            KeyName       = keyName;
            ReferenceName = refName;
            Start         = start;
            End           = end;
            JsonString    = jsonString;
            ReportingFor  = reportingFor;
        }

        public static SupplementaryInterval Read(BinaryReader reader)
        {
            var keyName       = reader.ReadString();
            var referenceName = reader.ReadString();
            var start         = reader.ReadInt32();
            var end           = reader.ReadInt32();
            var jsonString    = reader.ReadString();
            var reportingFor  = (ReportFor)reader.ReadByte();

            return new SupplementaryInterval(keyName, referenceName, start, end, jsonString, reportingFor);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(KeyName);
            writer.Write(ReferenceName);
            writer.Write(Start);
            writer.Write(End);
            writer.Write(JsonString);
            writer.Write((byte)ReportingFor);
        }

        public double? GetReciprocalOverlap(IInterval variant)
        {
            if (Start >= End || variant.Start > variant.End) return null;
            var overlapStart = Math.Max(Start, variant.Start);
            var overlapEnd   = Math.Min(End, variant.End);
            var maxLen       = Math.Max(variant.End - variant.Start + 1, End - Start + 1);
            return Math.Max(0, (overlapEnd - overlapStart + 1) * 1.0 / maxLen);
        }
    }
}