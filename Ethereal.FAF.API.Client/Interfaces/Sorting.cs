using Refit;
using System.ComponentModel;

namespace Ethereal.FAF.API.Client
{
    public class Sorting
    {
        public ListSortDirection SortDirection;
        [AliasAs("sort")]
        public string Sort
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Parameter) || Parameter is "None") return null;
                return
                    (SortDirection is ListSortDirection.Descending ?
                    "-" : string.Empty) +
                    Parameter;
            }
        }
        public string Parameter;
    }
}
