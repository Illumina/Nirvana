using System.Collections.Generic;
using System.Text;
using SAUtils;
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
            jsonSchema.AddAnnotation("name", new SaJsonKeyAnnotation { Type = "string" });
            jsonSchema.OutputKeyAnnotation("name");
            Assert.Equal("\"name\":{\"type\":\"string\"}", sb.ToString());
        }

        [Fact]
        public void ToString_AsExpected()
        {
            var jsonSchema = SaJsonSchema.Create(new StringBuilder(), "test", "array", new List<string> { "name", "phone", "employed" });
            jsonSchema.AddAnnotation("name", new SaJsonKeyAnnotation { Type = "string" });
            jsonSchema.AddAnnotation("phone", new SaJsonKeyAnnotation { Type = "number", Description = "phone number"});
            jsonSchema.AddAnnotation("employed", new SaJsonKeyAnnotation { Type = "boolean" });
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
    }
}