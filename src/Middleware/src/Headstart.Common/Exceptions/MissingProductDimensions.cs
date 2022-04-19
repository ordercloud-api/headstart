using System.Collections.Generic;

namespace Headstart.Common.Exceptions
{
	public class MissingProductDimensionsError
	{
		public IEnumerable<string> ProductIDsRequiringAttention { get; set; } = new List<string>();

		public MissingProductDimensionsError(IEnumerable<string> ids)
		{
			ProductIDsRequiringAttention = ids;
		}
	}
}