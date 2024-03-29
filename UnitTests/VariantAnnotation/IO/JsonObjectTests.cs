﻿using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;
using Xunit;

namespace UnitTests.VariantAnnotation.IO
{
    public sealed class JsonObjectTests
    {
        [Fact]
        public void ProcessBoolValue_True_TwoTimes()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);

            json.AddBoolValue("test1", true);
            json.AddBoolValue("test2", true);

            const string expectedResult = "\"test1\":true,\"test2\":true";
            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(expectedResult, observedResult);
        }


        [Fact]
        public void AddBoolValue_True_TwoTimes()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);
            json.AddBoolValue("test1", true);
            json.AddBoolValue("test2", true);

            const string expectedResult = "\"test1\":true,\"test2\":true";
            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void AddIntValue_TwoTimes()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);
            json.AddIntValue("test1", 5);
            json.AddIntValue("test2", 7);

            const string expectedResult = "\"test1\":5,\"test2\":7";
            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void AddIntValue_NullInt()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);
            json.AddIntValue("test1", null);

            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(string.Empty, observedResult);
        }

        [Fact]
        public void AddDoubleValue_TwoTimes()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);
            json.AddDoubleValue("test1", 5.7);
            json.AddDoubleValue("test2", 7.9);

            const string expectedResult = "\"test1\":5.7,\"test2\":7.9";
            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(expectedResult, observedResult);
        }
        
        public static string GetJsonDoubleString()
        {
            var defaultCulture = Thread.CurrentThread.CurrentCulture;
            var newCulture     = CultureInfo.CreateSpecificCulture("fr-FR");
            Thread.CurrentThread.CurrentCulture = newCulture;
            
            var sb   = StringBuilderPool.Get();
            var json = new JsonObject(sb);
            json.AddDoubleValue("test1", 5.7);
            json.AddDoubleValue("test2", 7.9);

            var result = StringBuilderPool.GetStringAndReturn(sb);
            Thread.CurrentThread.CurrentCulture = defaultCulture;
            
            return result;
        }
        [Fact]
        public void AddDoubleValue_InvariantCulture()
        {
            var task           = Task<string>.Factory.StartNew(GetJsonDoubleString);
            var observedResult = task.Result;
            
            const string expectedResult = "\"test1\":5.7,\"test2\":7.9";
            Assert.Equal(expectedResult, observedResult);

        }

        [Fact]
        public void AddDoubleValue_NullInt()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);
            json.AddDoubleValue("test1", null);

            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(string.Empty, observedResult);
        }

        [Fact]
        public void AddStringValue_TwoTimes()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);
            json.AddStringValue("test1", "bob");
            json.AddStringValue("test2", "jane", false);

            const string expectedResult = "\"test1\":\"bob\",\"test2\":jane";
            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void AddStringValue_NullInt()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);
            json.AddStringValue("test1", null);

            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(string.Empty, observedResult);
        }

        [Fact]
        public void AddStringValues_TwoTimes()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);

            var strings = new[] { "A", "B", "C" };
            var strings2 = new[] { "D", "E", "F" };

            json.AddStringValues("test1", strings);
            json.AddStringValues("test2", strings2, false);

            const string expectedResult = "\"test1\":[\"A\",\"B\",\"C\"],\"test2\":[D,E,F]";
            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void AddStringValues_NullArray()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);

            json.AddStringValues("test1", (string[])null);
            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(string.Empty, observedResult);
        }

        [Fact]
        public void AddIntValues_TwoTimes()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);

            var ints = new[] { 1, 2, 3 };
            var ints2 = new[] { 4, 5, 6 };

            json.AddIntValues("test1", ints);
            json.AddIntValues("test2", ints2);

            const string expectedResult = "\"test1\":[1,2,3],\"test2\":[4,5,6]";
            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void AddIntValues_NullArray()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);

            json.AddIntValues("test1", null);
            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(string.Empty, observedResult);
        }

        [Fact]
        public void AddObjectValues_TwoTimes()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);

            var points = new Point[2];
            points[0] = new Point(1, 2);
            points[1] = new Point(3, 4);

            var points2 = new Point[1];
            points2[0] = new Point(5, 6);

            json.AddObjectValues("test1", points);
            json.AddObjectValues("test2", points2);

            const string expectedResult = "\"test1\":[{\"X\":1,\"Y\":2},{\"X\":3,\"Y\":4}],\"test2\":[{\"X\":5,\"Y\":6}]";
            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void AddObjectValues_NullArray()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);

            json.AddObjectValues("test1", null as Point[]);
            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(string.Empty, observedResult);
        }

        [Fact]
        public void AddStringValues_EmptyArray()
        {
            var sb = StringBuilderPool.Get();
            var json = new JsonObject(sb);

            json.AddStringValues("test1", new string[0]);
            var observedResult = StringBuilderPool.GetStringAndReturn(sb);

            Assert.Equal(string.Empty, observedResult);
        }

        private sealed class Point : IJsonSerializer
        {
            private readonly int _x;
            private readonly int _y;

            public Point(int x, int y)
            {
                _x = x;
                _y = y;
            }

            public void SerializeJson(StringBuilder sb)
            {
                var jsonObject = new JsonObject(sb);
                sb.Append(JsonObject.OpenBrace);
                jsonObject.AddIntValue("X", _x);
                jsonObject.AddIntValue("Y", _y);
                sb.Append(JsonObject.CloseBrace);
            }
        }
    }
}
