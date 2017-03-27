#region Header
/**
 * JsonMapperTest.cs
 *   Tests for the JsonMapper class.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/
#endregion


using LitJson.LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace LitJson.Test
{
    // Sample classes to test json->object and object->json conversions
    public enum Planets
    {
        Jupiter,
        Saturn,
        Uranus,
        Neptune,
        Pluto
    }

    [Flags]
    public enum Instruments
    {
        Bass = 1,
        Guitar = 2,
        Drums = 4,
        Harp = 8
    }

    public class EnumsTest
    {
        public Planets FavouritePlanet;

        public Instruments Band;
    }

    public class NestedArrays
    {
        public int[][] numbers;

        public List<List<List<string>>> strings;
    }

    public class PropertyReadOnly
    {
        private int x;


        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return x * 2; }
        }
    }

    public class PropertyWriteOnly
    {
        private int x;


        public int X
        {
            set { x = value; }
        }


        public int GetX()
        {
            return x;
        }
    }

    public class UiImage
    {
        public string src;
        public string name;
        public int hOffset;
        public int vOffset;
        public string alignment;
    }

    public class UiSample
    {
        private UiWidget _widget;

        public UiWidget widget
        {
            get { return _widget; }
            set { _widget = value; }
        }

        public UiSample()
        {
            _widget = new UiWidget();
        }
    }

    public class UiText
    {
        public string data;
        public int size;
        public string style;
        public string name;
        public int hOffset;
        public int vOffset;
        public string alignment;
        public string onMouseUp;
    }

    public class UiWidget
    {
        private UiImage _image;
        private UiText _text;
        private UiWindow _window;

        public bool debug;

        public UiWindow window
        {
            get { return _window; }
            set { _window = value; }
        }

        public UiImage image
        {
            get { return _image; }
            set { _image = value; }
        }

        public UiText text
        {
            get { return _text; }
            set { _text = value; }
        }

        public UiWidget()
        {
            _image = new UiImage();
            _text = new UiText();
            _window = new UiWindow();
        }
    }

    public class UiWindow
    {
        public string title;
        public string name;
        public int width;
        public int height;
    }

    public class ValueTypesTest
    {
        public byte TestByte;
        public char TestChar;
        public DateTime TestDateTime;
        public decimal TestDecimal;
        public sbyte TestSByte;
        public short TestShort;
        public ushort TestUShort;
        public uint TestUInt;
        public ulong TestULong;
    }

    public class NullableTypesTest
    {
        public int? TestNullableInt;
    }

    public class CompoundNullableTypesTest
    {
        public CompoundNullableType<int>? TestNested;
        public CompoundNullableType<int>?[] TestNullableTypeArray;
    }

    public struct CompoundNullableType<T>
    {
        public T TestValue;
    }

    public enum NullableEnum
    {
        TestVal0 = 0,
        TestVal1 = 1,
        TestVal2 = 2
    }

    public class NullableEnumTest
    {
        public NullableEnum? TestEnum;
    }

    public class JsonMapperTest
    {
        [Fact]
        public void CustomExporterTest()
        {
            // Custom DateTime exporter that only uses the Year value
            ExporterFunc<DateTime> exporter =
                delegate (DateTime obj, JsonWriter writer)
                {
                    writer.Write(obj.Year);
                };

            JsonMapper.RegisterExporter<DateTime>(exporter);

            IDictionary sample = new Dictionary<string, DateTime>();

            sample.Add("date", new DateTime(1980, 12, 8));

            string json = JsonMapper.ToJson(sample);
            string expected = "{\"date\":1980}";

            JsonMapper.UnregisterExporters();

            Assert.Equal(expected, json);
        }

        [Fact]
        public void CustomImporterTest()
        {
            // Custom DateTime importer that only uses the Year value
            // (assuming January 1st of that year)
            ImporterFunc<int, DateTime> importer =
                delegate (int obj)
                {
                    return new DateTime(obj, 1, 1);
                };

            JsonMapper.RegisterImporter<int, DateTime>(importer);

            string json = "{ \"TestDateTime\" : 1980 }";

            ValueTypesTest sample =
                JsonMapper.ToObject<ValueTypesTest>(json);

            JsonMapper.UnregisterImporters();

            Assert.Equal(new DateTime(1980, 1, 1), sample.TestDateTime);
        }

        [Fact]
        public void EmptyObjectsTest()
        {
            JsonData empty_obj = JsonMapper.ToObject("{}");
            Assert.True(empty_obj.IsObject);

            string empty_json = JsonMapper.ToJson(empty_obj);
            Assert.Equal("{}", empty_json);

            JsonData empty_array = JsonMapper.ToObject("[]");
            Assert.True(empty_array.IsArray, "B1");

            empty_json = JsonMapper.ToJson(empty_array);
            Assert.Equal("[]", empty_json);
        }

        [Fact]
        public void ExportArrayOfIntsTest()
        {
            int[] numbers = new int[] { 1, 1, 2, 3, 5, 8, 13 };

            string json = JsonMapper.ToJson(numbers);

            Assert.Equal("[1,1,2,3,5,8,13]", json);
        }

        [Fact]
        public void ExportDictionaryTest()
        {
            IDictionary hash = new Dictionary<string, object>();

            hash.Add("product", "ACME rocket skates");
            hash.Add("quantity", 5);
            hash.Add("price", 45.95);

            string expected = "{\"product\":\"ACME rocket skates\"," +
                "\"quantity\":5,\"price\":45.95}";

            string json = JsonMapper.ToJson(hash);

            Assert.Equal(expected, json);
        }

        [Fact]
        public void ExportEnumsTest()
        {
            EnumsTest e_test = new EnumsTest();

            e_test.FavouritePlanet = Planets.Saturn;
            e_test.Band = Instruments.Bass | Instruments.Harp;

            string json = JsonMapper.ToJson(e_test);

            Assert.Equal("{\"FavouritePlanet\":1,\"Band\":9}", json);
        }

        [Fact]
        public void ExportObjectTest()
        {
            UiSample sample = new UiSample();

            sample.widget.window.title = "FooBar";
            sample.widget.window.name = "foo_window";
            sample.widget.window.width = 400;
            sample.widget.window.height = 300;

            sample.widget.image.src = "logo.png";
            sample.widget.image.name = "Foo Logo";
            sample.widget.image.hOffset = 10;
            sample.widget.image.vOffset = 20;
            sample.widget.image.alignment = "right";

            sample.widget.text.data = "About Us";
            sample.widget.text.size = 24;
            sample.widget.text.style = "normal";
            sample.widget.text.name = "about";
            sample.widget.text.alignment = "center";

            string expected = "{\"widget\":{\"window\":" +
                "{\"title\":\"FooBar\",\"name\":\"foo_window\"," +
                "\"width\":400,\"height\":300},\"image\":{\"src\":" +
                "\"logo.png\",\"name\":\"Foo Logo\",\"hOffset\":10," +
                "\"vOffset\":20,\"alignment\":\"right\"},\"text\":{" +
                "\"data\":\"About Us\",\"size\":24,\"style\":\"normal\"," +
                "\"name\":\"about\",\"hOffset\":0,\"vOffset\":0," +
                "\"alignment\":\"center\",\"onMouseUp\":null}," +
                "\"debug\":false}}";

            string json = JsonMapper.ToJson(sample);

            Assert.Equal(expected, json);
        }

        [Fact]
        public void ExportPrettyPrint()
        {
            var sample = new Dictionary<string, object>();

            sample["rolling"] = "stones";
            sample["flaming"] = "pie";
            sample["nine"] = 9;

            string expected = @"
{
    ""rolling"" : ""stones"",
    ""flaming"" : ""pie"",
    ""nine""    : 9
}";

            JsonWriter writer = new JsonWriter();
            writer.PrettyPrint = true;

            JsonMapper.ToJson(sample, writer);

            Assert.Equal(expected, writer.ToString());

            writer.Reset();
            writer.IndentValue = 8;

            expected = @"
{
        ""rolling"" : ""stones"",
        ""flaming"" : ""pie"",
        ""nine""    : 9
}";
            JsonMapper.ToJson(sample, writer);

            Assert.Equal(expected, writer.ToString());
        }

        [Fact]
        public void ExportValueTypesTest()
        {
            ValueTypesTest test = new ValueTypesTest();

            test.TestByte = 200;
            test.TestChar = 'P';
            test.TestDateTime = new DateTime(2012, 12, 22);
            test.TestDecimal = 10.333m;
            test.TestSByte = -5;
            test.TestShort = 1024;
            test.TestUShort = 30000;
            test.TestUInt = 90000000;
            test.TestULong = 0xFFFFFFFFFFFFFFFF; // = =18446744073709551615

            string json = JsonMapper.ToJson(test);
            string expected =
                "{\"TestByte\":200,\"TestChar\":\"P\",\"TestDateTime\":" +
                "\"12/22/2012 00:00:00\",\"TestDecimal\":10.333," +
                "\"TestSByte\":-5,\"TestShort\":1024,\"TestUShort\":30000" +
                ",\"TestUInt\":90000000,\"TestULong\":18446744073709551615}";

            Assert.Equal(expected, json);
        }

        [Fact]
        public void ImportArrayOfStringsTest()
        {
            string json = @"[
                ""Adam"",
                ""Danny"",
                ""James"",
                ""Justin""
            ]";

            string[] names = JsonMapper.ToObject<string[]>(json);

            Assert.True(names.Length == 4);
            Assert.Equal(names[1], "Danny");
        }

        [Fact]
        public void ImportEnumsTest()
        {
            string json = @"
                {
                    ""FavouritePlanet"" : 4,
                    ""Band"" : 6
                }";

            EnumsTest e_test = JsonMapper.ToObject<EnumsTest>(json);

            Assert.Equal(Planets.Pluto, e_test.FavouritePlanet);
            Assert.Equal(Instruments.Guitar
                             | Instruments.Drums, e_test.Band);
        }

        [Fact]
        public void ImportExtendedGrammarTest()
        {
            string json = @"
                {
                    // The domain name
                    ""domain"" : ""example.com"",

                    /******************
                     * The IP address *
                     ******************/
                    'ip_address' : '127.0.0.1'
                }
                ";

            JsonData data = JsonMapper.ToObject(json);

            Assert.Equal("example.com", (string)data["domain"]);
            Assert.Equal("127.0.0.1", (string)data["ip_address"]);
        }

        [Fact]
        public void ImportFromFileTest()
        {
            JsonData data;

            string path = Path.Combine(Directory.GetCurrentDirectory(), "json-example.txt");
            StreamReader stream = new StreamReader(new FileStream(path, FileMode.Open));

            using (stream)
            {
                data = JsonMapper.ToObject(stream);
            }

            Assert.Equal(
                "cofaxCDS",
                (string)data["web-app"]["servlet"][0]["servlet-name"]);
            Assert.Equal(
                false,
                (bool)data["web-app"]["servlet"][0]["init-param"]["useJSP"]);
            Assert.Equal(
                "cofax.tld",
                (string)data["web-app"]["taglib"]["taglib-uri"]);
        }

        [Fact]
        public void ImportJsonDataArrayTest()
        {
            string json = " [ 1, 10, 100, 1000 ] ";

            JsonData data = JsonMapper.ToObject(json);

            Assert.Equal(4, data.Count);
            Assert.Equal(1000, (int)data[3]);
        }

        [Fact]
        public void ImportManyJsonTextPiecesTest()
        {
            string json_arrays = @"
                [ true, true, false, false ]
                [ 10, 0, -10 ]
                [ ""war is over"", ""if you want it"" ]
                ";

            JsonReader reader;
            JsonData arrays;

            reader = new JsonReader(json_arrays);
            arrays = JsonMapper.ToObject(reader);

            Assert.False(reader.EndOfInput);

            Assert.True(arrays.IsArray);
            Assert.Equal(4, arrays.Count);
            Assert.Equal(true, (bool)arrays[0]);

            arrays = JsonMapper.ToObject(reader);

            Assert.False(reader.EndOfInput);

            Assert.True(arrays.IsArray);
            Assert.Equal(3, arrays.Count);
            Assert.Equal(10, (int)arrays[0]);

            arrays = JsonMapper.ToObject(reader);

            Assert.True(arrays.IsArray, "A9");
            Assert.Equal(2, arrays.Count);
            Assert.Equal("war is over", (string)arrays[0]);

            reader.Close();

            string json_objects = @"
                {
                  ""title""  : ""First"",
                  ""name""   : ""First Window"",
                  ""width""  : 640,
                  ""height"" : 480
                }

                {
                  ""title""  : ""Second"",
                  ""name""   : ""Second Window"",
                  ""width""  : 800,
                  ""height"" : 600
                }
                ";

            reader = new JsonReader(json_objects);
            UiWindow window;

            window = JsonMapper.ToObject<UiWindow>(reader);

            Assert.False(reader.EndOfInput, "A12");

            Assert.Equal("First", window.title);
            Assert.Equal(640, window.width);

            window = JsonMapper.ToObject<UiWindow>(reader);

            Assert.Equal("Second", window.title);
            Assert.Equal(800, window.width);

            reader.Close();

            // Read them in a loop to make sure we get the correct number of
            // iterations
            reader = new JsonReader(json_objects);

            int i = 0;

            while (!reader.EndOfInput)
            {
                window = JsonMapper.ToObject<UiWindow>(reader);
                i++;
            }

            Assert.Equal(2, i);
        }

        [Fact]
        public void ImportNestedArrays()
        {
            string json = "[ [ [ 42 ] ] ]";
            JsonData data = JsonMapper.ToObject(json);

            Assert.True(data.IsArray);
            Assert.Equal(1, data.Count);

            Assert.True(data[0].IsArray);
            Assert.Equal(1, data[0].Count);

            Assert.True(data[0][0].IsArray);
            Assert.Equal(1, data[0][0].Count);

            Assert.Equal(42, (int)data[0][0][0]);
            Assert.Equal("[[[42]]]", data.ToJson());

            json = "  [ [ 10, 20, 30 ], \"hi\", [ null, null ] ] ";
            data = JsonMapper.ToObject(json);

            Assert.True(data.IsArray, "B1");
            Assert.Equal(3, data.Count);

            Assert.Equal(20, (int)data[0][1]);
            Assert.Equal("hi", (string)data[1]);

            Assert.True(data[2].IsArray, "B5");
            Assert.Equal(2, data[2].Count);
            Assert.Null(data[2][0]);
            Assert.Null(data[2][1]);

            json = @"{
                ""numbers"" : [ [ 0, 1, 2 ], [], [ 2, 3, 5, 7, 11 ] ],
                ""strings"" : [
                    [ [ ""abc"", ""def"" ], [ ""hi there"" ], null ],
                    [ [ ""Bob Marley is in the house"" ] ]
                ]
            }";

            var obj = JsonMapper.ToObject<NestedArrays>(json);
            Assert.NotNull(obj);
            Assert.Equal(2, obj.numbers[0][2]);
            Assert.Equal(0, obj.numbers[1].Length);
            Assert.Equal(5, obj.numbers[2].Length);
            Assert.Equal(11, obj.numbers[2][4]);
            Assert.Equal("abc", obj.strings[0][0][0]);
            Assert.Equal("hi there", obj.strings[0][1][0]);
            Assert.Null(obj.strings[0][2]);
            Assert.Equal(1, obj.strings[1][0].Count);
        }

        [Fact]
        public void ImportNumbersTest()
        {
            double[] d_array;
            float[] f_array;
            decimal[] m_array;

            string json = " [ 0, 5, 10 ] ";

            d_array = JsonMapper.ToObject<double[]>(json);

            Assert.Equal(3, d_array.Length);
            Assert.Equal(10.0, d_array[2]);

            f_array = JsonMapper.ToObject<float[]>(json);

            Assert.Equal(3, f_array.Length);
            Assert.Equal(10.0, f_array[2]);

            m_array = JsonMapper.ToObject<decimal[]>(json);

            Assert.Equal(3, m_array.Length);
            Assert.Equal(10m, m_array[2]);
        }

        [Fact]
        public void ImportObjectTest()
        {
            string json = @"
{
  ""widget"": {
    ""debug"": true,

    ""window"": {
      ""title"": ""Sample Widget"",
      ""name"": ""main_window"",
      ""width"": 500,
      ""height"": 500
    },

    ""image"": {
      ""src"": ""Images/Sun.png"",
      ""name"": ""sun1"",
      ""hOffset"": 250,
      ""vOffset"": 250,
      ""alignment"": ""center""
    },

    ""text"": {
      ""data"": ""Click Here"",
      ""size"": 36,
      ""style"": ""bold"",
      ""name"": ""text1"",
      ""hOffset"": 250,
      ""vOffset"": 100,
      ""alignment"": ""center"",
      ""onMouseUp"": ""sun1.opacity = (sun1.opacity / 100) * 90;""
    }
  }
}";

            UiSample sample = JsonMapper.ToObject<UiSample>(json);

            Assert.NotNull(sample);
            Assert.Equal("Sample Widget", sample.widget.window.title);
            Assert.Equal(500, sample.widget.window.width);
            Assert.Equal("sun1", sample.widget.image.name);
            Assert.Equal("Click Here", sample.widget.text.data);
        }

        [Fact]
        public void ImportObjectSkipNonMembersTest()
        {
            string json = @"
{
    ""title""  : ""First"",

    ""extra_bool"": false,
    ""extra_object"":  {
      ""title""  : ""Sample Widget"",
      ""name""   : ""main_window"",
      ""width""  : 500,
      ""height"" : 500
    },

    ""name""   : ""First Window"",

    ""extra_array"" :[1, 2, 3],

    ""width""  : 640,

    ""extra_array_object"" : [
        {
            ""obj1"": { ""checked"": false },
            ""obj2"": [ 7, 6, 5 ]
        },
        {
            ""member1"": false,
            ""member2"": true,
            ""member3"": -1,
            ""member4"": ""vars2"",
            ""member5"": [9, 8, 7],
            ""member6"": { ""checked"": true }
        }
    ],

    ""height"" : 480

}";

            UiWindow window = JsonMapper.ToObject<UiWindow>(json);

            Assert.NotNull(window);
            Assert.Equal("First", window.title);
            Assert.Equal("First Window", window.name);
            Assert.Equal(640, window.width);
            Assert.Equal(480, window.height);
        }

        [Fact]
        //[ExpectedException(typeof(JsonException))]
        public void ImportObjectNonMembersTest()
        {
            string json = @"
{
    ""title""  : ""First"",

    ""extra_string"": ""Hello world"",

    ""name""   : ""First Window"",
    ""width""  : 640,
    ""height"" : 480

}";

            JsonReader reader = new JsonReader(json);
            reader.SkipNonMembers = false;

            UiWindow window = JsonMapper.ToObject<UiWindow>(reader);
            window.title = "Unreachable";
        }

        [Fact]
        //[ExpectedException(typeof(JsonException))]
        public void ImportStrictCommentsTest()
        {
            string json = @"
                [
                    /* This is a comment */
                    1,
                    2,
                    3
                ]";

            JsonReader reader = new JsonReader(json);
            reader.AllowComments = false;

            JsonData data = JsonMapper.ToObject(reader);

            if (data.Count != 3)
                data = JsonMapper.ToObject(reader);
        }

        [Fact]
        //[ExpectedException(typeof(JsonException))]
        public void ImportStrictStringsTest()
        {
            string json = "[ 'Look! Single quotes' ]";

            JsonReader reader = new JsonReader(json);
            reader.AllowSingleQuotedStrings = false;

            JsonData data = JsonMapper.ToObject(reader);

            if (data[0] == null)
                data = JsonMapper.ToObject(reader);
        }

        [Fact]
        public void ImportValueTypesTest()
        {
            string json = @"
{
  ""TestByte"":     200,
  ""TestChar"":     'P',
  ""TestDateTime"": ""12/22/2012 00:00:00"",
  ""TestDecimal"":  10.333,
  ""TestSByte"":    -5,
  ""TestShort"":    1024,
  ""TestUShort"":   30000,
  ""TestUInt"":     90000000,
  ""TestULong"":    18446744073709551615
}";

            ValueTypesTest test = JsonMapper.ToObject<ValueTypesTest>(json);

            Assert.Equal(200, test.TestByte);
            Assert.Equal('P', test.TestChar);
            Assert.Equal(new DateTime(2012, 12, 22),
                             test.TestDateTime);
            Assert.Equal(10.333m, test.TestDecimal);
            Assert.Equal(-5, test.TestSByte);
            Assert.Equal(1024, test.TestShort);
            Assert.Equal(30000, test.TestUShort);
            Assert.Equal((uint)90000000, test.TestUInt);
            Assert.Equal(18446744073709551615L, test.TestULong);
        }

        [Fact]
        public void NullConversionsTest()
        {
            object[] MyObjects = new object[] { "Hi!", 123, true, null };
            string json = JsonMapper.ToJson(MyObjects);

            Assert.Equal("[\"Hi!\",123,true,null]", json);

            JsonData data = JsonMapper.ToObject(json);

            Assert.Equal("Hi!", (string)data[0]);
            Assert.Equal(123, (int)data[1]);

            Assert.True((bool)data[2], "A4");
            Assert.Null(data[3]);
        }

        [Fact]
        public void PropertiesReadOnlyTest()
        {
            PropertyReadOnly p_obj = new PropertyReadOnly();

            p_obj.X = 10;

            string json = JsonMapper.ToJson(p_obj);

            Assert.Equal("{\"X\":10,\"Y\":20}", json);

            PropertyReadOnly p_obj2 =
                JsonMapper.ToObject<PropertyReadOnly>(json);

            Assert.Equal(10, p_obj2.X);
            Assert.Equal(20, p_obj2.Y);
        }

        [Fact]
        public void PropertiesWriteOnlyTest()
        {
            string json = " { \"X\" : 3 } ";

            PropertyWriteOnly p_obj =
                JsonMapper.ToObject<PropertyWriteOnly>(json);

            Assert.Equal(3, p_obj.GetX());

            json = JsonMapper.ToJson(p_obj);

            Assert.Equal("{}", json);
        }

        [Fact]
        public void NullableTypesImportTest()
        {
            string json = @" {
                ""TestNullableInt"": 42
            }";
            var value = JsonMapper.ToObject<NullableTypesTest>(json);
            Assert.Equal(value.TestNullableInt, 42);

            json = @" {
                ""TestNullableInt"": null
            }";
            value = JsonMapper.ToObject<NullableTypesTest>(json);
            Assert.Equal(value.TestNullableInt, null);
        }

        [Fact]
        public void NullableTypesExportTest()
        {
            string expectedJson = "{\"TestNullableInt\":42}";
            var value = new NullableTypesTest() { TestNullableInt = 42 };
            Assert.Equal(expectedJson, JsonMapper.ToJson(value));

            expectedJson = "{\"TestNullableInt\":null}";
            value = new NullableTypesTest() { TestNullableInt = null };
            Assert.Equal(expectedJson, JsonMapper.ToJson(value));
        }

        [Fact]
        public void CompoundNullableTypesImportTest()
        {
            string json = @" {
                ""TestNested"": {
                    ""TestValue"": 42
                }
            }";
            JsonReader reader = new JsonReader(json);
            reader.SkipNonMembers = false;
            var value = JsonMapper.ToObject<CompoundNullableTypesTest>(reader);
            Assert.NotEqual(value.TestNested, null);
            var innerValue = (CompoundNullableType<int>)value.TestNested;
            Assert.Equal(innerValue.TestValue, 42);

            json = @" {
                ""TestNested"": null
            }";
            value = JsonMapper.ToObject<CompoundNullableTypesTest>(json);
            Assert.Equal(value.TestNested, null);

            json = @" {
                ""TestNullableTypeArray"": [
                    { ""TestValue"": 42 },
                    { ""TestValue"": 43 },
                    { ""TestValue"": 44 }
                ]
            }";
            value = JsonMapper.ToObject<CompoundNullableTypesTest>(json);
            Assert.NotEqual(value.TestNullableTypeArray, null);
            Assert.Equal(value.TestNullableTypeArray.Length, 3);

            Assert.NotEqual(value.TestNullableTypeArray[0], null);
            innerValue =
                (CompoundNullableType<int>)value.TestNullableTypeArray[0];
            Assert.Equal(innerValue.TestValue, 42);

            Assert.NotEqual(value.TestNullableTypeArray[1], null);
            innerValue =
                (CompoundNullableType<int>)value.TestNullableTypeArray[1];
            Assert.Equal(innerValue.TestValue, 43);

            Assert.NotEqual(value.TestNullableTypeArray[2], null);
            innerValue =
                (CompoundNullableType<int>)value.TestNullableTypeArray[2];
            Assert.Equal(innerValue.TestValue, 44);
        }

        [Fact]
        public void CompoundNullableTypesExportTest()
        {
            CompoundNullableTypesTest value = new CompoundNullableTypesTest()
            {
                TestNested = new CompoundNullableType<int>() { TestValue = 42 }
            };
            var expectedJson =
                "{\"TestNested\":{\"TestValue\":42}," +
                "\"TestNullableTypeArray\":null}";
            Assert.Equal(expectedJson, JsonMapper.ToJson(value));

            value = new CompoundNullableTypesTest()
            {
                TestNullableTypeArray = new[] {
                    new Nullable<CompoundNullableType<int>>(
                        new CompoundNullableType<int>() { TestValue = 42 }),
                    new Nullable<CompoundNullableType<int>>(
                        new CompoundNullableType<int>() { TestValue = 43 }),
                    new Nullable<CompoundNullableType<int>>(
                        new CompoundNullableType<int>() { TestValue = 44 })
                }
            };
            expectedJson =
                "{\"TestNested\":null,\"TestNullableTypeArray\":" +
                "[{\"TestValue\":42},{\"TestValue\":43},{\"TestValue\":44}]}";
            Assert.Equal(expectedJson, JsonMapper.ToJson(value));
        }

        [Fact]
        public void NullableEnumImportTest()
        {
            string json = @"{
                ""TestEnum"": 1
            }";
            var value = JsonMapper.ToObject<NullableEnumTest>(json);
            Assert.NotEqual(value.TestEnum, null);
            var enumValue = (NullableEnum)value.TestEnum;
            Assert.Equal(enumValue, NullableEnum.TestVal1);

            json = @"{
                ""TestEnum"": null
            }";
            value = JsonMapper.ToObject<NullableEnumTest>(json);
            Assert.Equal(value.TestEnum, null);
        }

        [Fact]
        public void NullableEnumExportTest()
        {
            var value = new NullableEnumTest()
            {
                TestEnum = NullableEnum.TestVal2
            };
            string expectedJson = "{\"TestEnum\":2}";
            Assert.Equal(expectedJson, JsonMapper.ToJson(value));

            value = new NullableEnumTest() { TestEnum = null };
            expectedJson = "{\"TestEnum\":null}";
            Assert.Equal(expectedJson, JsonMapper.ToJson(value));
        }
    }
}
