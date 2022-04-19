using Headstart.Common.Models.Base;

namespace Headstart.Common.Services.CMS.Models
{
	public class Document<T> : HsBaseObject
	{
		public T Doc { get; set; }

		public string SchemaSpecUrl { get; set; } = string.Empty;

		public History History { get; set; } = new History();
	}
}