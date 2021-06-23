using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RegionOrebroLan.Web.Authentication.OpenIdConnect
{
	/// <summary>
	/// https://openid.net/specs/openid-connect-core-1_0.html#ClaimsParameter
	/// </summary>
	public class ClaimsRequest
	{
		#region Properties

		[JsonPropertyName("id_token")]
		[SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
		public virtual IDictionary<string, ClaimsRequestItem> IdToken { get; set; }

		[JsonPropertyName("userinfo")]
		[SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
		public virtual IDictionary<string, ClaimsRequestItem> UserInfo { get; set; }

		#endregion

		#region Methods

		public virtual string ToJson(bool ignoreNullValues = true, bool indent = false)
		{
			return JsonSerializer.Serialize(this, this.GetType(), new JsonSerializerOptions
			{
				IgnoreNullValues = ignoreNullValues,
				WriteIndented = indent
			});
		}

		public override string ToString()
		{
			return this.ToJson();
		}

		#endregion
	}
}