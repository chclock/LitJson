#region Header
/**
 * JsonDataTest.cs
 *   Tests for the JsonData class.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/
#endregion


using System;
using System.Collections.Generic;
using Xunit;

namespace LitJson.Test
{
    public class JsonDataTest
    {
        [Fact]
        public void AsArrayTest()
        {
            JsonData data = new JsonData();

            data.Add(1);
            data.Add(2);
            data.Add(3);
            data.Add("Launch!");

            Assert.True(data.IsArray);
            Assert.Equal("[1,2,3,\"Launch!\"]", data.ToJson());
        }

        [Fact]
        public void AsBooleanTest()
        {
            JsonData data;

            data = true;
            Assert.True(data.IsBoolean);
            Assert.True((bool)data);
            Assert.Equal("true", data.ToJson());

            data = false;
            bool f = false;

            Assert.Equal(f, (bool)data);
        }

        [Fact]
        public void AsDoubleTest()
        {
            JsonData data;

            data = 3e6;
            Assert.True(data.IsDouble);
            Assert.Equal(3e6, (double)data);
            Assert.Equal("3000000.0", data.ToJson());

            data = 3.14;
            Assert.True(data.IsDouble, "A4");
            Assert.Equal(3.14, (double)data);
            Assert.Equal("3.14", data.ToJson());

            data = 0.123;
            double n = 0.123;

            Assert.Equal(n, (double)data);
        }

        [Fact]
        public void AsIntTest()
        {
            JsonData data;

            data = 13;
            Assert.True(data.IsInt);
            Assert.Equal((int)data, 13);
            Assert.Equal(data.ToJson(), "13");

            data = -00500;

            Assert.True(data.IsInt, "A4");
            Assert.Equal((int)data, -500);
            Assert.Equal(data.ToJson(), "-500");

            data = 1024;
            int n = 1024;

            Assert.Equal((int)data, n);
        }

        [Fact]
        public void AsObjectTest()
        {
            JsonData data = new JsonData();

            data["alignment"] = "left";
            data["font"] = new JsonData();
            data["font"]["name"] = "Arial";
            data["font"]["style"] = "italic";
            data["font"]["size"] = 10;
            data["font"]["color"] = "#fff";

            Assert.True(data.IsObject);

            string json = "{\"alignment\":\"left\",\"font\":{" +
                "\"name\":\"Arial\",\"style\":\"italic\",\"size\":10," +
                "\"color\":\"#fff\"}}";

            Assert.Equal(json, data.ToJson());
        }

        [Fact]
        public void AsStringTest()
        {
            JsonData data;

            data = "All you need is love";
            Assert.True(data.IsString);
            Assert.Equal("All you need is love", (string)data);
            Assert.Equal("\"All you need is love\"", data.ToJson());
        }

        [Fact]
        public void EqualsTest()
        {
            JsonData a;
            JsonData b;

            // Compare ints
            a = 7;
            b = 7;
            Assert.True(a.Equals(b));

            Assert.False(a.Equals(null));

            b = 8;
            Assert.False(a.Equals(b));

            // Compare longs
            a = 10L;
            b = 10L;
            Assert.True(a.Equals(b), "A4");

            b = 10;
            Assert.False(a.Equals(b));
            b = 11L;
            Assert.False(a.Equals(b));

            // Compare doubles
            a = 78.9;
            b = 78.9;
            Assert.True(a.Equals(b));

            b = 78.899999;
            Assert.False(a.Equals(b));

            // Compare booleans
            a = true;
            b = true;
            Assert.True(a.Equals(b), "A9");

            b = false;
            Assert.False(a.Equals(b), "A10");

            // Compare strings
            a = "walrus";
            b = "walrus";
            Assert.True(a.Equals(b), "A11");

            b = "Walrus";
            Assert.False(a.Equals(b), "A12");
        }

        [Fact]
        public void GetKeysTest()
        {
            JsonData data = new JsonData();

            data["first"] = "one";
            data["second"] = "two";
            data["third"] = "three";
            data["fourth"] = "four";

            Assert.Equal(4, data.Keys.Count);

            foreach (string k in data.Keys)
                Assert.NotNull(data[k]);
        }

        [Fact]
        public void GetKeysInvalidTypeTest()
        {
            Assert.Throws(typeof(InvalidOperationException), () =>
            {
                JsonData data = new JsonData();
                data.Add(42);  // turns it into an array

                // .. but an array doesn't have keys
                ICollection<string> keys = data.Keys;
                Assert.NotNull(keys);
            });
        }

        [Fact]
        public void InvalidCastTest()
        {
            Assert.Throws(typeof(InvalidCastException), ()=> {
                JsonData data = 35;
                string str = (string)data;
            });
        }

        [Fact]
        public void NullValue()
        {
            string json = "{\"test\":null}";

            JsonData data = new JsonData();
            data["test"] = null;

            Assert.Equal(json, data.ToJson());
        }

        [Fact]
        public void PropertiesOrderTest()
        {
            JsonData data = new JsonData();

            string json = "{\"first\":\"one\",\"second\":\"two\"," +
                "\"third\":\"three\",\"fourth\":\"four\"}";

            for (int i = 0; i < 10; i++)
            {
                data.Clear();

                data["first"] = "one";
                data["second"] = "two";
                data["third"] = "three";
                data["fourth"] = "four";

                Assert.Equal(json, data.ToJson());
            }
        }
    }
}
