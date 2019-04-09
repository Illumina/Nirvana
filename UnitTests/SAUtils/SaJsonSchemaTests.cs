using System.Collections.Generic;
using System.Text;
using ErrorHandling.Exceptions;
using SAUtils.Schema;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.SAUtils
{
    public sealed class SaJsonSchemaTests
    {
        private const string SchemaVersion = "http://json-schema.org/draft-06/schema#";

        [Fact]
        public void Create_InitialJsonObject_AsExpected()
        {
            var sb = new StringBuilder();
            SaJsonSchema.Create(sb, "test", SaJsonValueType.ObjectArray, new List<string>());
            const string expectedJsonString = "{\"$schema\":\"" + SchemaVersion + "\",\"type\":\"object\",\"properties\":{\"test\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{";
            Assert.Equal(expectedJsonString, sb.ToString());
        }

        [Fact]
        public void OutputKeyAnnotation_AsExpected()
        {
            var sb = new StringBuilder();
            var jsonSchema = new SaJsonSchema(sb);
            jsonSchema.AddAnnotation("name", SaJsonKeyAnnotation.CreateFromProperties(SaJsonValueType.String, 0, null));
            jsonSchema.OutputKeyAnnotation("name");
            Assert.Equal("\"name\":{\"type\":\"string\"}", sb.ToString());
        }

        [Fact]
        public void ToString_AsExpected()
        {
            var jsonSchema = SaJsonSchema.Create(new StringBuilder(), "test", SaJsonValueType.ObjectArray, new List<string> { "name", "phone", "employed" });
            jsonSchema.AddAnnotation("name", SaJsonKeyAnnotation.CreateFromProperties(SaJsonValueType.String, 0, null));
            jsonSchema.AddAnnotation("phone", SaJsonKeyAnnotation.CreateFromProperties(SaJsonValueType.Number, 0, "phone number"));
            jsonSchema.AddAnnotation("employed", SaJsonKeyAnnotation.CreateFromProperties(SaJsonValueType.Bool, 0, null));
            jsonSchema.TotalItems = 100;
            jsonSchema.KeyCounts["name"] = 100;
            jsonSchema.KeyCounts["phone"] = 50;
            jsonSchema.KeyCounts["employed"] = 0;

            const string expectedJsonSchemaString = "{\"$schema\":\"" + SchemaVersion + "\",\"type\":\"object\",\"properties\":{\"test\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{"
                                                  + "\"name\":{\"type\":\"string\"},\"phone\":{\"type\":\"number\",\"description\":\"phone number\"}},"
                                                  + "\"required\":[\"name\"],\"additionalProperties\":false}}}}";

            Assert.Equal(expectedJsonSchemaString, jsonSchema.ToString());
            // make sure the returned string is the same when ToString method is called more than once
            Assert.Equal(expectedJsonSchemaString, jsonSchema.ToString());
        }

        [Fact]
        public void GetJsonString_AsExpected()
        {
            var jsonSchema = SaJsonSchema.Create(new StringBuilder(), "test", SaJsonValueType.ObjectArray, new List<string> { "name", "phone", "employed" });
            jsonSchema.AddAnnotation("name", SaJsonKeyAnnotation.CreateFromProperties(SaJsonValueType.String, 0, null));
            jsonSchema.AddAnnotation("phone", SaJsonKeyAnnotation.CreateFromProperties(SaJsonValueType.Number, 0, "phone number"));
            jsonSchema.AddAnnotation("employed", SaJsonKeyAnnotation.CreateFromProperties(SaJsonValueType.Bool, 0, null));
            var jsonString = jsonSchema.GetJsonString(new List<string[]> { new[] { "Ada" }, new[] { "123456" }, new[] { "true" } });

            Assert.Equal("\"name\":\"Ada\",\"phone\":123456,\"employed\":true", jsonString);
        }

        [Fact]
        public void GetJsonString_DoubleValueHandling_AsExpected()
        {
            var jsonSchema = SaJsonSchema.Create(new StringBuilder(), "test", SaJsonValueType.ObjectArray, new List<string> { "allAf", "doubleValue1", "doubleValue2" });
            jsonSchema.AddAnnotation("allAf", SaJsonKeyAnnotation.CreateFromProperties(SaJsonValueType.Number, CustomAnnotationCategories.AlleleFrequency, null));
            jsonSchema.AddAnnotation("doubleValue1", SaJsonKeyAnnotation.CreateFromProperties(SaJsonValueType.Number, 0, "A double value"));
            jsonSchema.AddAnnotation("doubleValue2", SaJsonKeyAnnotation.CreateFromProperties(SaJsonValueType.Number, 0, "Another double value"));
            var jsonString = jsonSchema.GetJsonString(new List<string[]> { new[] { "0.12345678" }, new[] { "0.12" }, new[] { "0.12345678" } });

            Assert.Equal("\"allAf\":0.123457,\"doubleValue1\":0.12,\"doubleValue2\":0.12345678", jsonString);
        }

        [Fact]
        public void CheckAndGetBoolFromString_AsExpected()
        {
            Assert.True(SaJsonSchema.CheckAndGetBoolFromString("true"));
            Assert.True(SaJsonSchema.CheckAndGetBoolFromString("TRUE"));
            Assert.False(SaJsonSchema.CheckAndGetBoolFromString("false"));
            Assert.False(SaJsonSchema.CheckAndGetBoolFromString("False"));
            Assert.False(SaJsonSchema.CheckAndGetBoolFromString(""));
            Assert.False(SaJsonSchema.CheckAndGetBoolFromString("."));
        }

        [Fact]
        public void CheckAndGetBoolFromString_InvalidValue_ThrowException()
        {
            Assert.Throws<UserErrorException>(() => SaJsonSchema.CheckAndGetBoolFromString("T"));
            Assert.Throws<UserErrorException>(() => SaJsonSchema.CheckAndGetBoolFromString("F"));
            Assert.Throws<UserErrorException>(() => SaJsonSchema.CheckAndGetBoolFromString("0"));
            Assert.Throws<UserErrorException>(() => SaJsonSchema.CheckAndGetBoolFromString("-"));
        }

        [Fact]
        public void CheckAndGetNullableDoubleFromString_GetNull_AsExpected()
        {
            Assert.Null(SaJsonSchema.CheckAndGetNullableDoubleFromString(""));
            Assert.Null(SaJsonSchema.CheckAndGetNullableDoubleFromString("."));
        }

        [Fact]
        public void CheckAndGetNullableDoubleFromString_NotANum_ThrowException()
        {
            Assert.Throws<UserErrorException>(() => SaJsonSchema.CheckAndGetNullableDoubleFromString("Bob"));
            Assert.Throws<UserErrorException>(() => SaJsonSchema.CheckAndGetNullableDoubleFromString("1+1"));
            Assert.Throws<UserErrorException>(() => SaJsonSchema.CheckAndGetNullableDoubleFromString("bool"));
        }

        [Fact]
        public void GetJsonString__AsExpected()
        {
            var jsonSchema = SaJsonSchema.Create(new StringBuilder(), "test", SaJsonValueType.ObjectArray, new List<string> { "allAf", "doubleValue1", "doubleValue2" });
            jsonSchema.AddAnnotation("allAf", SaJsonKeyAnnotation.CreateFromProperties(SaJsonValueType.Number, CustomAnnotationCategories.AlleleFrequency, null ));
            jsonSchema.AddAnnotation("doubleValue1", SaJsonKeyAnnotation.CreateFromProperties(SaJsonValueType.Number, 0, "A double value" ));
            jsonSchema.AddAnnotation("doubleValue2", SaJsonKeyAnnotation.CreateFromProperties(SaJsonValueType.Number, 0, "Another double value" ));
            var jsonString = jsonSchema.GetJsonString(new List<string[]> { new[] { "0.12345678" }, new[] { "0.12" }, new[] { "0.12345678" } });

            Assert.Equal("\"allAf\":0.123457,\"doubleValue1\":0.12,\"doubleValue2\":0.12345678", jsonString);
        }
    }
}