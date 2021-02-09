using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Services.CMS.Models
{
	public class Document<T>
	{
		public string ID { get; set; }
		public T Doc { get; set; }
		public string SchemaSpecUrl { get; set; }
		public History History { get; set; }
	}
}
