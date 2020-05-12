using System.Linq;

namespace NetCore.Extensions.Core.Data
{
    public interface IDescriptor
    {
        void Deserialize(string source);

        string Serialize();
    }

    public class SortDescriptor : IDescriptor
    {
        /// <summary>
        ///     Gets or sets the member name which will be used for sorting.
        /// </summary>
        public string Member { get; set; }

        /// <summary>
        ///     Gets or sets the sort direction for this sort descriptor. If the value is null no sorting will be applied.
        /// </summary>
        public SortDirection SortDirection { get; set; }

        public void Deserialize(string source)
        {
            var strArray = source.Split(new[]
            {
                '-'
            });
            if (strArray.Length > 1)
                Member = strArray[0];
            SortDirection = strArray.Last() == "desc" ? SortDirection.Descending : SortDirection.Ascending;
        }

        public string Serialize()
        {
            return string.Format("{0}-{1}", Member, SortDirection == SortDirection.Ascending ? "asc" : "desc");
        }
    }
}
