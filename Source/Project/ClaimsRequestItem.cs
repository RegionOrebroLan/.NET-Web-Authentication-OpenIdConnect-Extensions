using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RegionOrebroLan.Web.Authentication.OpenIdConnect
{
	public class ClaimsRequestItem
	{
		#region Properties

		[JsonPropertyName("essential")]
		public virtual bool? Essential { get; set; }

		[JsonPropertyName("value")]
		public virtual string Value { get; set; }

		[JsonPropertyName("values")]
		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		public virtual string[] Values { get; set; }

		#endregion
	}
}