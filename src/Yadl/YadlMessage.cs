using System;

namespace Yadl
{
    public class YadlMessage
    {
        public string Message { get; set; }
        public int Level { get; set; }
        public string LevelDescription { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public string ExtraFields { get; set; }
    }
}