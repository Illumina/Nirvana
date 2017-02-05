using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public sealed class DbSnpAnnotation : ISupplementaryAnnotation
    {
        #region members

        public List<long> DbSnp = new List<long>();
        public double AltAlleleFreq = double.MinValue;

        #endregion

        public bool HasConflicts { get; }

        public void Read(ExtendedBinaryReader reader)
        {
            var count = reader.ReadOptInt32();
            for (var i = 0; i < count; i++) DbSnp.Add(reader.ReadOptInt64());
        }

        public void AddAnnotationToVariant(IAnnotatedAlternateAllele jsonVariant)
        {
            if (DbSnp == null) return;

            var newDbSnp = new List<string>();
            foreach (var dbSnp in DbSnp) newDbSnp.Add("rs" + dbSnp);
            jsonVariant.DbSnpIds = newDbSnp.ToArray();
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            if (DbSnp == null) DbSnp = new List<long>();
            writer.WriteOpt(DbSnp.Count);
            foreach (var id in DbSnp) writer.WriteOpt(id);
        }

        public void MergeAnnotations(ISupplementaryAnnotation other)
        {
            var otherAnnotation = other as DbSnpAnnotation;
            if (otherAnnotation?.DbSnp == null || otherAnnotation.DbSnp.Count == 0)
                return;

            DbSnp.AddRange(otherAnnotation.DbSnp.Where(x => !DbSnp.Contains(x)));

            if (otherAnnotation.AltAlleleFreq > double.MinValue) AltAlleleFreq = otherAnnotation.AltAlleleFreq;

        }

        public void Clear()
        {
            DbSnp.Clear();
        }
    }
}