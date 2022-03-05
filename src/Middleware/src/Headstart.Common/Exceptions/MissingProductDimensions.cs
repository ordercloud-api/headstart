using System.Collections.Generic;

namespace Headstart.Models.Exceptions
{
    public class MissingProductDimensionsError
    {
        public IEnumerable<string> ProductIDsRequiringAttention { get; set; }

        public MissingProductDimensionsError(IEnumerable<string> ids)
        {
            ProductIDsRequiringAttention = ids;
        }
    }
}