#region Header
/**
 * JsonWriterTest.cs
 *   Tests for the JsonWriter class.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/
#endregion


using System;
using System.IO;
using System.Text;
using Xunit;

namespace LitJson.Test
{
    public class JsonWriterTest
    {
        [Fact]
        public void BooleansTest()
        {
            JsonWriter writer = new JsonWriter();

            writer.WriteArrayStart();
            writer.Write(true);
            writer.Write(false);
            writer.Write(false);
            writer.Write(true);
            writer.WriteArrayEnd();
        }

        [Fact]
        public void ErrorExcessDataTest()
        {
            Assert.Throws(typeof(JsonException), () =>
            {
                JsonWriter writer = new JsonWriter();

                writer.WriteArrayStart();
                writer.Write(true);
                writer.WriteArrayEnd();
                writer.Write(false);
            });
        }

        [Fact]
        public void ErrorArrayClosingTest()
        {
            Assert.Throws(typeof(JsonException), () =>
            {
                JsonWriter writer = new JsonWriter();

                writer.WriteArrayStart();
                writer.Write(true);
                writer.WriteObjectEnd();
            });
        }

        [Fact]
        public void ErrorNoArrayOrObjectTest()
        {
            Assert.Throws(typeof(JsonException), () =>
            {
                JsonWriter writer = new JsonWriter();

                writer.Write(true);
            });
        }

        [Fact]
        public void ErrorObjectClosingTest()
        {
            Assert.Throws(typeof(JsonException), () =>
            {
                JsonWriter writer = new JsonWriter();

                writer.WriteObjectStart();
                writer.WritePropertyName("foo");
                writer.Write("bar");
                writer.WriteArrayEnd();
            });
        }

        [Fact]
        public void ErrorPropertyExpectedTest()
        {
            Assert.Throws(typeof(JsonException), () =>
            {
                JsonWriter writer = new JsonWriter();

                writer.WriteObjectStart();
                writer.Write(10);
                writer.WriteObjectEnd();
            });
        }

        [Fact]
        public void ErrorValueExpectedTest()
        {
            Assert.Throws(typeof(JsonException), () =>
            {
                JsonWriter writer = new JsonWriter();

                writer.WriteObjectStart();
                writer.WritePropertyName("foo");
                writer.WriteObjectEnd();
            });
        }

        [Fact]
        public void NestedArraysTest()
        {
            JsonWriter writer = new JsonWriter();

            string json = "[1,[\"a\",\"b\",\"c\"],2,[[null]],3]";

            writer.WriteArrayStart();
            writer.Write(1);
            writer.WriteArrayStart();
            writer.Write("a");
            writer.Write("b");
            writer.Write("c");
            writer.WriteArrayEnd();
            writer.Write(2);
            writer.WriteArrayStart();
            writer.WriteArrayStart();
            writer.Write(null);
            writer.WriteArrayEnd();
            writer.WriteArrayEnd();
            writer.Write(3);
            writer.WriteArrayEnd();

            Assert.Equal(writer.ToString(), json);
        }

        [Fact]
        public void NestedObjectsTest()
        {
            JsonWriter writer = new JsonWriter();

            string json = "{\"book\":{\"title\":" +
                "\"Structure and Interpretation of Computer Programs\"," +
                "\"details\":{\"pages\":657}}}";

            writer.WriteObjectStart();
            writer.WritePropertyName("book");
            writer.WriteObjectStart();
            writer.WritePropertyName("title");
            writer.Write(
                "Structure and Interpretation of Computer Programs");
            writer.WritePropertyName("details");
            writer.WriteObjectStart();
            writer.WritePropertyName("pages");
            writer.Write(657);
            writer.WriteObjectEnd();
            writer.WriteObjectEnd();
            writer.WriteObjectEnd();

            Assert.Equal(writer.ToString(), json);
        }

        [Fact]
        public void NullWriterTest()
        {
            Assert.Throws(typeof(ArgumentNullException), () =>
            {
                TextWriter text_writer = null;
                JsonWriter writer = new JsonWriter(text_writer);

                writer.Write(123);
            });
        }

        [Fact]
        public void NullTest()
        {
            JsonWriter writer = new JsonWriter();

            writer.WriteArrayStart();
            writer.Write(null);
            writer.WriteArrayEnd();
        }

        [Fact]
        public void NullTextWriterTest()
        {
            JsonWriter writer = new JsonWriter(TextWriter.Null);

            writer.WriteArrayStart();
            writer.Write("Hello");
            writer.Write("World");
            writer.WriteArrayEnd();
        }

        [Fact]
        public void NumbersTest()
        {
            JsonWriter writer = new JsonWriter();

            writer.WriteArrayStart();
            writer.Write(0);
            writer.Write(100);
            writer.Write((byte)200);
            writer.Write(-256);
            writer.Write(10000000000L);
            writer.Write((decimal)0.333);
            writer.Write((float)0.0001);
            writer.Write(9e-20);
            writer.Write(2.3e8);
            writer.Write(Math.PI);
            writer.WriteArrayEnd();
        }

        [Fact]
        public void ObjectTest()
        {
            JsonWriter writer = new JsonWriter();

            string json = "{\"flavour\":\"strawberry\",\"color\":\"red\"," +
                "\"amount\":3}";

            writer.WriteObjectStart();
            writer.WritePropertyName("flavour");
            writer.Write("strawberry");
            writer.WritePropertyName("color");
            writer.Write("red");
            writer.WritePropertyName("amount");
            writer.Write(3);
            writer.WriteObjectEnd();

            Assert.Equal(writer.ToString(), json);
        }

        [Fact]
        public void PrettyPrintTest()
        {
            JsonWriter writer = new JsonWriter();

            string json = @"
[
    {
        ""precision"" : ""zip"",
        ""Latitude""  : 37.7668,
        ""Longitude"" : -122.3959,
        ""City""      : ""SAN FRANCISCO""
    },
  {
    ""precision"" : ""zip"",
    ""Latitude""  : 37.371991,
    ""Longitude"" : -122.02602,
    ""City""      : ""SUNNYVALE""
  }
]";

            writer.PrettyPrint = true;

            writer.WriteArrayStart();
            writer.WriteObjectStart();
            writer.WritePropertyName("precision");
            writer.Write("zip");
            writer.WritePropertyName("Latitude");
            writer.Write(37.7668);
            writer.WritePropertyName("Longitude");
            writer.Write(-122.3959);
            writer.WritePropertyName("City");
            writer.Write("SAN FRANCISCO");
            writer.WriteObjectEnd();

            writer.IndentValue = 2;

            writer.WriteObjectStart();
            writer.WritePropertyName("precision");
            writer.Write("zip");
            writer.WritePropertyName("Latitude");
            writer.Write(37.371991);
            writer.WritePropertyName("Longitude");
            writer.Write(-122.02602);
            writer.WritePropertyName("City");
            writer.Write("SUNNYVALE");
            writer.WriteObjectEnd();
            writer.WriteArrayEnd();

            Assert.Equal(writer.ToString(), json);
        }

        [Fact]
        public void StringBuilderTest()
        {
            StringBuilder sb = new StringBuilder();
            JsonWriter writer = new JsonWriter(sb);

            writer.WriteArrayStart();
            writer.Write("like a lizard on a window pane");
            writer.WriteArrayEnd();

            Assert.Equal(sb.ToString(),
                             "[\"like a lizard on a window pane\"]");
        }

        [Fact]
        public void StringsTest()
        {
            JsonWriter writer = new JsonWriter();

            writer.WriteArrayStart();
            writer.Write("Hello World!");
            writer.Write("\n\r\b\f\t");
            writer.Write("I \u2665 you");
            writer.Write("She said, \"I know what it's like to be dead\"");
            writer.WriteArrayEnd();

            string json =
                "[\"Hello World!\",\"\\n\\r\\b\\f\\t\",\"I \\u2665 you\"" +
                ",\"She said, \\\"I know what it's like to be dead\\\"\"]";

            Assert.Equal(json, writer.ToString());
        }
    }
}
