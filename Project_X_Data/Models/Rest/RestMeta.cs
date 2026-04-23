using System.Data;

namespace Project_X_Data.Models.Rest
{
    public class RestMeta
    {
        public long ServerTime { get; set; } = DateTime.Now.Ticks;
        public String Service { get; set; }
        public String Url { get; set; }
        public long Cache { get; set; }
        public String[] Manipulations { get; set; } = [];
        public String DataType { get; set; }
        public Dictionary<String, String> Links { get; set; } = [];
        public Dictionary<String, Object> Opt { get; set; } = [];
    }
}
