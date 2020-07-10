using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using Yadl.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Yadl.Tests
{
    public class JsonTest
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JsonTest()
        {
            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.Converters.Add(new IEnumerableKeyValuePairConverter());
        }

        private class A
        {
            public string? Field1 { get; set; }
            public string? Field2 { get; set; }
            public B? NestedObject { get; set; }

            public class B
            {
                public int? NestedField { get; set; }
                public B? NestedObject { get; set; }
            }
        }

        [Fact]
        public void Merge_Different_JsonPath()
        {
            var json1 = JsonSerializer.Serialize(new A
            {
                Field1 = "Value1",
                NestedObject = new A.B
                {
                    NestedField = 1
                }
            });

            var json2 = JsonSerializer.Serialize(new A
            {
                Field2 = "Value2",
                NestedObject = new A.B
                {
                    NestedObject = new A.B
                    {
                        NestedField = 200
                    }
                }
            });

            var mergedJson = JsonExtensions.Merge(json1, json2);
            Assert.NotNull(mergedJson);
            Assert.NotEmpty(mergedJson);
            
            var finalJson = JsonSerializer.Deserialize<A>(mergedJson);
            Assert.Equal("Value1", finalJson.Field1);
            Assert.Equal("Value2", finalJson.Field2);
            
            Assert.NotNull(finalJson.NestedObject);
            Assert.Equal(1, finalJson.NestedObject?.NestedField);
            
            Assert.NotNull(finalJson.NestedObject?.NestedObject);
            Assert.Equal(200, finalJson.NestedObject?.NestedObject?.NestedField);
        }

        [Fact]
        public void MergeOptions()
        {
            var json1 = JsonSerializer.Serialize(new A
            {
                Field1 = "Value1",
                Field2 = "Value2",
                NestedObject = new A.B
                {
                    NestedField = 1
                }
            });

            var json2 = JsonSerializer.Serialize(new A
            {
                NestedObject = new A.B
                {
                    NestedField = 100
                }
            });

            var mergedJson = JsonExtensions.Merge(json1, json2);
            var finalJson = JsonSerializer.Deserialize<A>(mergedJson);

            Assert.NotNull(mergedJson);
            Assert.NotEmpty(mergedJson);
            Assert.Equal(100, finalJson.NestedObject?.NestedField);
        }

        [Fact]
        public void VerifyDictionaryConverter()
        {
            var dic = new Dictionary<string, object>
            {
                {"Field1", "Value1"},
                {"Field2", "Value2"},
                {
                    "NestedObject", new Dictionary<string, object>
                    {
                        {"NestedField", 1}
                    }
                }
            };

            var expected = @"{""Field1"":""Value1"",""Field2"":""Value2"",""NestedObject"":{""NestedField"":1}}";

            {
                var dicString = JsonSerializer.Serialize(dic, _jsonSerializerOptions);
                Assert.NotEmpty(dicString);
                Assert.Equal(expected, dicString);
            }

            {
                var dicString = JsonConvert.SerializeObject(dic);
                Assert.NotEmpty(dicString);
                Assert.Equal(expected, dicString);
            }
        }
    }
}