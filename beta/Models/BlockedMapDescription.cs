using beta.Models.Enums;

namespace beta.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class BlockedMapDescription
    {
        public BlockedMapDescription(string name, FilterDescription filter)
        {
            Name = name;
            Filter = filter;
        }

        public string Name { get; }
        public FilterDescription Filter { get; }

    }
}
