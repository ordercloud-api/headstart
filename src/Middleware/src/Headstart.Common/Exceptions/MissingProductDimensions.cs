using System.Collections.Generic;

namespace Headstart.Models.Exceptions
{
	public class MissingProductDimensionsError
	{
		public MissingProductDimensionsError(IEnumerable<string> ids)
		{
			ProductIDsRequiringAttention = ids;
		}

		public IEnumerable<string> ProductIDsRequiringAttention;
	}
}
