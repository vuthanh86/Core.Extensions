namespace NetCore.Extensions.Core.Data
{
    public class RoboUIGridClientRequest
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string Sorts { get; set; }

        public string Filters { get; set; }
    }
}
