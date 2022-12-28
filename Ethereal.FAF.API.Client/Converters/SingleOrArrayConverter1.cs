using System.Collections.Generic;

namespace beta.Infrastructure.Converters.JSON
{
    public class SingleOrArrayConverter<TItem> : SingleOrArrayConverter<List<TItem>, TItem>
    {
        public SingleOrArrayConverter() : this(true) { }
        public SingleOrArrayConverter(bool canWrite) : base(canWrite) { }
    }
}
