using System;
using System.Collections.Generic;

namespace RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration
{
	public class ClaimsRequestMapOptions
	{
		#region Properties

		public virtual IList<ClaimsRequestMappingOptions> Mappings { get; } = new List<ClaimsRequestMappingOptions>();

		#endregion

		#region Methods

		public virtual IDictionary<string, ClaimsRequest> ToDictionary()
		{
			var dictionary = new Dictionary<string, ClaimsRequest>(StringComparer.OrdinalIgnoreCase);

			try
			{
				foreach(var mapping in this.Mappings)
				{
					var claimsRequest = mapping.ClaimsRequest?.ToClaimsRequest();

					dictionary.Add(mapping.AuthenticationScheme, claimsRequest);
				}
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException("Could not create dictionary from mappings.", exception);
			}

			return dictionary;
		}

		#endregion
	}
}