using System;
using IO;
using VariantAnnotation.PSA;

namespace VariantAnnotation.SA
{
    public record SaSignature(string Identifier, int MagicNumber)
    {
        private static readonly Random _random = new Random();
        public                  string Identifier  { get; } = Identifier;
        public                  int    MagicNumber { get; } = MagicNumber;

        public static SaSignature Generate(string identifier)
        {
            var magicNumber = _random.Next(1_000_000, int.MaxValue);
            return new SaSignature(identifier, magicNumber);
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.Write(SaCommon.GuardInt);
            writer.WriteOptAscii(Identifier);
            writer.WriteOpt(MagicNumber);
            
        }
        
        public static SaSignature Read(ExtendedBinaryReader reader)
        {
            PsaUtilities.CheckGuardInt(reader, "SaSignature");
            var identifier  = reader.ReadAsciiString();
            var magicNumber = reader.ReadOptInt32();

            return new SaSignature(identifier, magicNumber);
        }
    }
}