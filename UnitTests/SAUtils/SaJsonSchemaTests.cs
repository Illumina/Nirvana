using System.Collections.Generic;
using System.Text;
using ErrorHandling.Exceptions;
using SAUtils;
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
            SaJsonSchema.Create(sb, "test", "array", new List<string>());
            const string expectedJsonString = "{\"$schema\":\"" + SchemaVersion + "\",\"type\":\"object\",\"properties\":{\"test\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{";
            Assert.Equal(expectedJsonString, sb.ToString());
        }

        [Fact]
        public void OutputKeyAnnotation_AsExpected()
        {
            var sb = new StringBuilder();
            var jsonSchema = new SaJsonSchema(sb);
            jsonSchema.AddAnnotation("name", new SaJsonKeyAnnotation { Type = JsonDataType.String });
            jsonSchema.OutputKeyAnnotation("name");
            Assert.Equal("\"name\":{\"type\":\"string\"}", sb.ToString());
        }

        [Fact]
        public void ToString_AsExpected()
        {
            var jsonSchema = SaJsonSchema.Create(new StringBuilder(), "test", "array", new List<string> { "name", "phone", "employed" });
            jsonSchema.AddAnnotation("name", new SaJsonKeyAnnotation { Type = JsonDataType.String });
            jsonSchema.AddAnnotation("phone", new SaJsonKeyAnnotation { Type = JsonDataType.Number, Description = "phone number"});
            jsonSchema.AddAnnotation("employed", new SaJsonKeyAnnotation { Type =JsonDataType.Bool });
            jsonSchema.TotalItems = 100;
            jsonSchema.KeyCounts["name"] = 100;
            jsonSchema.KeyCounts["phone"] = 50;
            jsonSchema.KeyCounts["employed"] = 0;

            const string expectedJsonSchemaString = "{\"$schema\":\"" + SchemaVersion + "\",\"type\":\"object\",\"properties\":{\"test\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{"
                                                  + "\"name\":{\"type\":\"string\"},\"phone\":{\"type\":\"number\",\"description\":\"phone number\"}},"
                                                  + "\"required\":[\"name\"]}}}}";

            Assert.Equal(expectedJsonSchemaString, jsonSchema.ToString());
            // make sure the returned string is the same when ToString method is called more than once
            Assert.Equal(expectedJsonSchemaString, jsonSchema.ToString());
        }

        [Fact]
        public void GetJsonString_AsExpected()
        {
            var jsonSchema = SaJsonSchema.Create(new StringBuilder(), "test", "array", new List<string> { "name", "phone", "employed" });
            jsonSchema.AddAnnotation("name", new SaJsonKeyAnnotation { Type = JsonDataType.String });
            jsonSchema.AddAnnotation("phone", new SaJsonKeyAnnotation { Type = JsonDataType.Number, Description = "phone number" });
            jsonSchema.AddAnnotation("employed", new SaJsonKeyAnnotation { Type = JsonDataType.Bool });
            var jsonString = jsonSchema.GetJsonString(new List<string> { "Ada", "123456", "true" });
            
            Assert.Equal("\"name\":\"Ada\",\"phone\":123456,\"employed\":true", jsonString);
        }

        [Fact]
        public void GetJsonString_DoubleValueHandling_AsExpected()
        {
            var jsonSchema = SaJsonSchema.Create(new StringBuilder(), "test", "array", new List<string> { "allAf", "doubleValue1", "doubleValue2"});
            jsonSchema.AddAnnotation("allAf", new SaJsonKeyAnnotation { Type = JsonDataType.Number, Category = CustomAnnotationCategories.AlleleFrequency});
            jsonSchema.AddAnnotation("doubleValue1", new SaJsonKeyAnnotation { Type = JsonDataType.Number, Description = "A double value" });
            jsonSchema.AddAnnotation("doubleValue2", new SaJsonKeyAnnotation { Type = JsonDataType.Number, Description = "Another double value" });
            var jsonString = jsonSchema.GetJsonString(new List<string> { "0.12345678", "0.12", "0.12345678" });

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
            var jsonSchema = SaJsonSchema.Create(new StringBuilder(), "test", "array", new List<string> { "allAf", "doubleValue1", "doubleValue2" });
            jsonSchema.AddAnnotation("allAf", new SaJsonKeyAnnotation { Type = JsonDataType.Number, Category = CustomAnnotationCategories.AlleleFrequency });
            jsonSchema.AddAnnotation("doubleValue1", new SaJsonKeyAnnotation { Type = JsonDataType.Number, Description = "A double value" });
            jsonSchema.AddAnnotation("doubleValue2", new SaJsonKeyAnnotation { Type = JsonDataType.Number, Description = "Another double value" });
            var jsonString = jsonSchema.GetJsonString(new List<string> { "0.12345678", "0.12", "0.12345678" });

            Assert.Equal("\"allAf\":0.123457,\"doubleValue1\":0.12,\"doubleValue2\":0.12345678", jsonString);
        }
    }
}