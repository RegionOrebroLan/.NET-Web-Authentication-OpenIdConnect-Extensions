using System.Diagnostics.CodeAnalysis;

namespace RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration
{
	public class ClaimsRequestItemOptions
	{
		#region Properties

		public virtual bool? Essential { get; set; }
		public virtual string Key { get; set; }
		public virtual string Value { get; set; }

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		public virtual string[] Values { get; set; }

		#endregion
	}
}