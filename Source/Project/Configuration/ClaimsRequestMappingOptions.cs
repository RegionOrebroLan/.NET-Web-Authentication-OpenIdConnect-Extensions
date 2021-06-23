namespace RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration
{
	public class ClaimsRequestMappingOptions
	{
		#region Properties

		public virtual string AuthenticationScheme { get; set; }
		public virtual ClaimsRequestOptions ClaimsRequest { get; set; }

		#endregion
	}
}