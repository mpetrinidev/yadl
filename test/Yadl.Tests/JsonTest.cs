using System.Collections.Generic;
using System.Text.Json;
using Newtonsoft.Json;
using Xunit;
using Yadl.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Yadl.Tests
{
    public class JsonTest
    {
        [Fact]
        public static void VerifyDictionaryConverter()
        {
            var jsonOptions = new JsonSerializerOptions();
            var dictionaryConverter = new DictionaryConverter();
            jsonOptions.Converters.Add(dictionaryConverter);

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
                var dicString = JsonSerializer.Serialize(dic, jsonOptions);
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