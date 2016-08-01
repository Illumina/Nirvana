using System;
using System.IO;
using System.Text;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.DataStructures
{
    public sealed class AnnotationRecordTests
    {
        [Fact]
        public void AllelicEncodedDataLoop()
        {
            foreach (AlleleSpecificId allelicId in Enum.GetValues(typeof (AlleleSpecificId)))
            {
                byte alleleIdByte = (byte) allelicId;

                foreach (AnnotationRecordDataType dataType in Enum.GetValues(typeof (AnnotationRecordDataType)))
                {
                    byte expectedEncodedData = AnnotationRecordCommon.GetEncodedData(dataType, alleleIdByte);

                    var newDataType = AnnotationRecordCommon.GetDataType(expectedEncodedData);
                    var newId = AnnotationRecordCommon.GetId(expectedEncodedData);

                    byte observedEncodedData = AnnotationRecordCommon.GetEncodedData(newDataType, newId);

                    Assert.Equal(expectedEncodedData, observedEncodedData);
                }
            }
        }

        [Fact]
        public void BooleanReferenceMinorAllele()
        {
            const bool expectedValue = true;
            var expectedBooleanRecord = new BooleanRecord((byte) PositionalId.IsRefMinorAllele, expectedValue);
            const byte expectedId = (byte) PositionalId.IsRefMinorAllele;

            Assert.Equal(expectedBooleanRecord.Id, expectedId);

            AbstractAnnotationRecord abstractAnnotationRecord;

            using (var ms = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(ms, Encoding.UTF8, true))
                {
                    var writer = new ExtendedBinaryWriter(binaryWriter);
                    expectedBooleanRecord.Write(writer);
                }

                ms.Seek(0, SeekOrigin.Begin);

                using (var binaryReader = new BinaryReader(ms))
                {
                    var reader = new ExtendedBinaryReader(binaryReader);
                    abstractAnnotationRecord = AbstractAnnotationRecord.Read(reader);
                }
            }

            Assert.Equal(expectedId, abstractAnnotationRecord.Id);

            var observedBooleanRecord = abstractAnnotationRecord as BooleanRecord;
            Assert.NotNull(observedBooleanRecord);

            if (observedBooleanRecord != null)
            {
                Assert.Equal(expectedValue, observedBooleanRecord.Value);
            }
        }

        [Fact]
        public void PositionalEncodedDataLoop()
        {
            foreach (PositionalId positionalId in Enum.GetValues(typeof (PositionalId)))
            {
                byte positionalIdByte = (byte) positionalId;

                foreach (AnnotationRecordDataType dataType in Enum.GetValues(typeof (AnnotationRecordDataType)))
                {
                    byte expectedEncodedData = AnnotationRecordCommon.GetEncodedData(dataType, positionalIdByte);

                    var newDataType = AnnotationRecordCommon.GetDataType(expectedEncodedData);
                    var newId = AnnotationRecordCommon.GetId(expectedEncodedData);

                    byte observedEncodedData = AnnotationRecordCommon.GetEncodedData(newDataType, newId);

                    Assert.Equal(expectedEncodedData, observedEncodedData);
                }
            }
        }
    }
}