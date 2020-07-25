using System.Collections.Generic;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using Yadl.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Yadl.PerformanceTests
{
    [MemoryDiagnoser]
    public class JsonBenchmark
    {
        private Dictionary<string, object> _dic;
        private JsonSerializerOptions _jsonOptions;

        [GlobalSetup]
        public void Setup()
        {
            _jsonOptions = new JsonSerializerOptions();
            _jsonOptions.Converters.Add(new DictionaryConverter());

            _dic = new Dictionary<string, object>
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
        }

        [Benchmark(Baseline = true, Description = "Serialize using System.Text.Json")]
        public void DictionaryToString1() => JsonSerializer.Serialize(_dic, _jsonOptions);
        
        [Benchmark(Description = "Serialize using Json.NET")]
        public void DictionaryToString2() => JsonConvert.SerializeObject(_dic);
    }
}