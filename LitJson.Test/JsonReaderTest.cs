#region Header
/**
 * JsonReaderTest.cs
 *   Tests for the JsonReader class.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/
#endregion


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Xunit;

namespace LitJson.Test
{
    public class JsonReaderTest
    {
        [Fact]
        public void BooleanTest()
        {
            string json = "[ true, false ]";

            JsonReader reader = new JsonReader(json);
            reader.Read();

            reader.Read();
            Assert.True((bool)reader.Value);
            reader.Read();
            Assert.True(!((bool)reader.Value));

            reader.Close();
        }

        [Fact]
        public void CommentsTest()
        {
            string json = @"
                {
                    // This is the first property
                    ""foo"" : ""bar"",

                    /**
                     * This is the second property
                     **/
                     ""baz"": ""blah""
                }";

            JsonReader reader = new JsonReader(json);

            reader.Read();
            reader.Read();
            Assert.Equal("foo", (string)reader.Value);

            reader.Read();
            reader.Read();
            Assert.Equal("baz", (string)reader.Value);

            reader.Read();
            reader.Read();
            Assert.True(reader.EndOfJson);
        }

        [Fact]
        public void DoubleTest()
        {
            string json = @"[ 0.0, -0.0, 3.1416, 8e-3, 7E-5, -128.000009,
                   144e+3, 0.1e2 ]";

            JsonReader reader = new JsonReader(json);
            reader.Read();

            reader.Read();
            Assert.Equal((double)reader.Value, 0.0,
                             new DoubleCompare());
            reader.Read();
            Assert.Equal((double)reader.Value, 0.0,
                             new DoubleCompare());
            reader.Read();
            Assert.Equal((double)reader.Value, 3.1416,
                             new DoubleCompare());
            reader.Read();
            Assert.Equal((double)reader.Value, 0.008,
                             new DoubleCompare());
            reader.Read();
            Assert.Equal((double)reader.Value, 0.00007,
                             new DoubleCompare());
            reader.Read();
            Assert.Equal((double)reader.Value, -128.000009,
                             new DoubleCompare());
            reader.Read();
            Assert.Equal((double)reader.Value, 144000.0,
                             new DoubleCompare());
            reader.Read();
            Assert.Equal((double)reader.Value, 10.0,
                             new DoubleCompare());

            reader.Close();
        }

        [Fact]
        public void EmptyStringTest()
        {
            string json = "[ \"\" ]";

            JsonReader reader = new JsonReader(json);
            reader.Read();

            reader.Read();
            Assert.Equal(reader.Value, String.Empty);

            reader.Close();
        }

        [Fact]
        public void EndOfJsonTest()
        {
            string json = " [ 1 ] [ 2, 3 ] [ 4, 5, 6 ] ";

            JsonReader reader = new JsonReader(json);

            int i;
            for (i = 0; i < 3; i++)
            {
                Assert.False(reader.EndOfJson);
                reader.Read();
            }

            Assert.True(reader.EndOfJson);
            Assert.False(reader.EndOfInput);

            reader.Read();

            for (i = 0; i < 3; i++)
            {
                Assert.False(reader.EndOfJson);
                reader.Read();
            }

            Assert.True(reader.EndOfJson);
            Assert.False(reader.EndOfInput);

            reader.Read();

            for (i = 0; i < 4; i++)
            {
                Assert.False(reader.EndOfJson);
                reader.Read();
            }

            Assert.True(reader.EndOfJson);

            reader.Read();
            Assert.True(reader.EndOfInput, "A9");
        }

        [Fact]
        public void FromFileTest()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "json-example.txt");
            StreamReader stream = new StreamReader(new FileStream(path, FileMode.Open));

            JsonReader reader = new JsonReader(stream);

            while (reader.Read()) ;
        }

        [Fact]
        public void IntTest()
        {
            string json = "[ 0, -0, 123, 14400, -500 ]";

            JsonReader reader = new JsonReader(json);
            reader.Read();

            reader.Read();
            Assert.Equal((int)reader.Value, 0);
            reader.Read();
            Assert.Equal((int)reader.Value, 0);
            reader.Read();
            Assert.Equal((int)reader.Value, 123);
            reader.Read();
            Assert.Equal((int)reader.Value, 14400);
            reader.Read();
            Assert.Equal((int)reader.Value, -500);

            reader.Close();
        }

        [Fact]
        //[ExpectedException(typeof(JsonException))]
        public void LexerErrorEscapeSequenceTest()
        {
            string json = "[ \"Hello World \\ufffg \" ]";

            JsonReader reader = new JsonReader(json);

            while (reader.Read()) ;
        }

        [Fact]
        //[ExpectedException(typeof(JsonException))]
        public void LexerErrorRealNumberTest()
        {
            // One ore more digits have to appear after the '.'
            string json = "[ 0.e5 ]";

            JsonReader reader = new JsonReader(json);

            while (reader.Read()) ;
        }

        [Fact]
        //[ExpectedException(typeof(JsonException))]
        public void LexerErrorTrueTest()
        {
            string json = "[ TRUE ]";

            JsonReader reader = new JsonReader(json);

            while (reader.Read()) ;
        }

        [Fact]
        [Category("RuntimeBug")]  // Int32.TryParse in mono 1.2.5
        public void LongTest()
        {
            string json = "[ 2147483648, -10000000000 ]";

            JsonReader reader = new JsonReader(json);
            reader.Read();

            reader.Read();
            Assert.Equal(typeof(Int64), reader.Value.GetType());
            Assert.Equal(2147483648L, (long)reader.Value);
            reader.Read();
            Assert.Equal(-10000000000L, (long)reader.Value);

            reader.Close();
        }

        [Fact]
        public void NestedArrays()
        {
            string json = "[ [ [ [ [ 1, 2, 3 ] ] ] ] ]";

            int array_count = 0;

            JsonReader reader = new JsonReader(json);

            while (reader.Read())
            {
                if (reader.Token == JsonToken.ArrayStart)
                    array_count++;
            }

            Assert.Equal(array_count, 5);
        }

        [Fact]
        public void NestedObjects()
        {
            string json = "{ \"obj1\": { \"obj2\": { \"obj3\": true } } }";

            int object_count = 0;
            JsonReader reader = new JsonReader(json);

            while (reader.Read())
            {
                if (reader.Token == JsonToken.ObjectStart)
                    object_count++;
            }

            Assert.Equal(object_count, 3);
        }

        [Fact]
        //[ExpectedException(typeof(ArgumentNullException))]
        public void NullReaderTest()
        {
            TextReader text_reader = null;
            JsonReader reader = new JsonReader(text_reader);

            while (reader.Read()) ;
        }

        [Fact]
        public void NullTest()
        {
            string json = "[ null ]";

            JsonReader reader = new JsonReader(json);
            reader.Read();

            reader.Read();
            Assert.Null(reader.Value);

            reader.Close();
        }

        [Fact]
        //[ExpectedException(typeof(JsonException))]
        public void ParserErrorArrayClosingTest()
        {
            string json = "[ 1, 2, 3 }";

            JsonReader reader = new JsonReader(json);

            while (reader.Read()) ;
        }

        [Fact]
        //[ExpectedException(typeof(JsonException))]
        public void ParserErrorIncompleteObjectTest()
        {
            string json = "{ \"temperature\" : 21 ";

            JsonReader reader = new JsonReader(json);

            while (reader.Read()) ;
        }

        [Fact]
        //[ExpectedException(typeof(JsonException))]
        public void ParserErrorNoArrayOrObjectTest()
        {
            string json = "true";

            JsonReader reader = new JsonReader(json);

            while (reader.Read()) ;
        }

        [Fact]
        //[ExpectedException(typeof(JsonException))]
        public void ParserErrorObjectClosingTest()
        {
            string json = @"{
                ""sports"": [
                    ""football"", ""baseball"", ""basketball"" ] ]";

            JsonReader reader = new JsonReader(json);

            while (reader.Read()) ;
        }

        [Fact]
        //[ExpectedException(typeof(JsonException))]
        public void ParserErrorPropertyExpectedTest()
        {
            string json = "{ {\"foo\": bar} }";

            JsonReader reader = new JsonReader(json);

            while (reader.Read()) ;
        }

        [Fact]
        public void QuickArrayTest()
        {
            string json = "[ \"George\", \"John\", \"Ringo\", \"Paul\" ]";

            JsonReader reader = new JsonReader(json);

            reader.Read();
            Assert.Equal(reader.Token, JsonToken.ArrayStart);
            reader.Read();
            Assert.Equal(reader.Token, JsonToken.String);
            Assert.Equal(reader.Value, "George");
            reader.Read();
            Assert.Equal(reader.Token, JsonToken.String);
            Assert.Equal(reader.Value, "John");
            reader.Read();
            Assert.Equal(reader.Token, JsonToken.String);
            Assert.Equal(reader.Value, "Ringo");
            reader.Read();
            Assert.Equal(reader.Token, JsonToken.String);
            Assert.Equal(reader.Value, "Paul");
            reader.Read();
            Assert.Equal(reader.Token, JsonToken.ArrayEnd);
            reader.Read();
            Assert.True(reader.EndOfJson, "A11");
        }

        [Fact]
        public void QuickObjectTest()
        {
            string json = @"{
                ""vehicle"": ""submarine"",
                ""color"":   ""yellow""
            }";

            JsonReader reader = new JsonReader(json);

            reader.Read();
            Assert.Equal(reader.Token, JsonToken.ObjectStart);
            reader.Read();
            Assert.Equal(reader.Token, JsonToken.PropertyName);
            Assert.Equal(reader.Value, "vehicle");
            reader.Read();
            Assert.Equal(reader.Token, JsonToken.String);
            Assert.Equal(reader.Value, "submarine");
            reader.Read();
            Assert.Equal(reader.Token, JsonToken.PropertyName);
            Assert.Equal(reader.Value, "color");
            reader.Read();
            Assert.Equal(reader.Token, JsonToken.String);
            Assert.Equal(reader.Value, "yellow");
            reader.Read();
            Assert.Equal(reader.Token, JsonToken.ObjectEnd);
            reader.Read();
            Assert.True(reader.EndOfJson, "A11");
        }

        [Fact]
        //[ExpectedException (typeof (JsonException))]
        public void StrictCommentsTest()
        {
            string json = @"
                [
                    // This is a comment
                    1,
                    2,
                    3
                ]";

            JsonReader reader = new JsonReader(json);
            reader.AllowComments = false;

            while (reader.Read()) ;
        }

        [Fact]
        //[ExpectedException (typeof (JsonException))]
        public void StrictStringsTest()
        {
            string json = "[ 'Look! Single quotes' ]";

            JsonReader reader = new JsonReader(json);
            reader.AllowSingleQuotedStrings = false;

            while (reader.Read()) ;
        }

        [Fact]
        public void StringsTest()
        {
            string json =
                "[ \"abc 123 \\n\\f\\b\\t\\r \\\" \\\\ \\u263a \\u25CF\" ]";

            string str = "abc 123 \n\f\b\t\r \" \\ \u263a \u25cf";

            JsonReader reader = new JsonReader(json);
            reader.Read();
            reader.Read();

            Assert.Equal(str, reader.Value);

            reader.Close();

            json = " [ '\"Hello\" \\'world\\'' ] ";
            str = "\"Hello\" 'world'";

            reader = new JsonReader(json);
            reader.Read();
            reader.Read();

            Assert.Equal(str, reader.Value);

            reader.Close();
        }
    }

    internal class DoubleCompare : IEqualityComparer<double>
    {
        public bool Equals(double x, double y)
        {
            return Math.Abs(x - y) <= Double.Epsilon * 2;
        }

        public int GetHashCode(double obj)
        {
            return obj.GetHashCode();
        }
    }
}
