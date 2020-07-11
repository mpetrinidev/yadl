using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using BenchmarkDotNet.Attributes;
using FastMember;

namespace Yadl.PerformanceTests
{
    [MemoryDiagnoser]
    public class DataReaderBenchmark
    {
        private ConcurrentBag<YadlMessage> _yadlMessages;

        [GlobalSetup]
        public void Setup()
        {
            _yadlMessages = new ConcurrentBag<YadlMessage>(Enumerable.Repeat<YadlMessage>(new YadlMessage
            {
                Message = "Msg N",
                Level = 1,
                LevelDescription = "Debug",
                ExtraFields = null
            }, 100_000));
        }

        [Benchmark(Baseline = true, Description = "ObjectReader using FastMember")]
        public ObjectReader CreateObjectReader() => ObjectReader.Create(_yadlMessages, "Id",
            "Message",
            "Level",
            "LevelDescription",
            "TimeStamp",
            "ExtraFields");

        [Benchmark(Description = "Using DataTable")]
        public DataTable CreateDataTable()
        {
            var dt = new DataTable("Logs");
            dt.Columns.Add(new DataColumn("Message"));
            dt.Columns.Add(new DataColumn("Level"));
            dt.Columns.Add(new DataColumn("LevelDescription"));
            dt.Columns.Add(new DataColumn("TimeStamp"));
            dt.Columns.Add(new DataColumn("ExtraFields"));
            
            foreach (var message in _yadlMessages)
            {
                var row = dt.NewRow();
                row["Message"] = message.Message;
                row["Level"] = message.Level;
                row["LevelDescription"] = message.LevelDescription;
                row["TimeStamp"] = message.TimeStamp;
                row["ExtraFields"] = message.ExtraFields;

                dt.Rows.Add(row);
            }
            
            return dt;
        }
    }
}