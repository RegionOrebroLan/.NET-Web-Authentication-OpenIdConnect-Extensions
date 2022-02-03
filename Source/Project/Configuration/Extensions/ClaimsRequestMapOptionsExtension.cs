using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using RegionOrebroLan.Logging.Extensions;

namespace RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration.Extensions
{
	public static class ClaimsRequestMapOptionsExtension
	{
		#region Methods

		[SuppressMessage("Design", "CA1021:Avoid out parameters")]
		public static bool TryGetClaimsRequestJson(this ClaimsRequestMapOptions claimsRequestMapOptions, string authenticationScheme, out string claimsRequestJson, ILogger logger = null, string logItemName = "open-id-connect-options")
		{
			if(claimsRequestMapOptions == null)
				throw new ArgumentNullException(nameof(claimsRequestMapOptions));

			if(authenticationScheme == null)
				throw new ArgumentNullException(nameof(authenticationScheme));

			claimsRequestJson = null;

			var claimsRequestMap = claimsRequestMapOptions.ToDictionary();

			if(claimsRequestMap.TryGetValue(authenticationScheme, out var claimsRequest))
			{
				var idToken = claimsRequest.IdToken ?? new Dictionary<string, ClaimsRequestItem>();
				var userInfo = claimsRequest.UserInfo ?? new Dictionary<string, ClaimsRequestItem>();

				if(!idToken.Any() && !userInfo.Any())
					claimsRequest = null;
			}

			if(claimsRequest == null)
			{
				logger?.LogDebugIfEnabled($"There is no claims-request configured for {logItemName} \"{authenticationScheme}\".");

				return false;
			}

			claimsRequestJson = claimsRequest.ToJson();

			logger?.LogDebugIfEnabled($"Claims-request for {logItemName} \"{authenticationScheme}\" is \"{claimsRequestJson}\".");

			return true;
		}

		#endregion
	}
}