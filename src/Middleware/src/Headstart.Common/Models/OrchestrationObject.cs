using Headstart.Common.Models.Base;

namespace Headstart.Common.Models
{
	public interface IOrchestrationObject<T>
	{
		string Id { get; set; }

		string Token { get; set; }

		string ClientId { get; set; }
	}

	public class OrchestrationObject<T> : IOrchestrationObject<T> where T : HsBaseObject
	{
		public string Id { get; set; } = string.Empty;

		public string Token { get; set; } = string.Empty;

		public string ClientId { get; set; } = string.Empty;

		public HsBaseObject Model { get; set; }
	}
}