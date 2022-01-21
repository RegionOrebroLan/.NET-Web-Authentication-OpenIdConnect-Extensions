using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Logging.Extensions;

namespace RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration
{
	public class PostConfigureOpenIdConnectClaimsRequestOptions : IPostConfigureOptions<OpenIdConnectOptions>
	{
		#region Constructors

		public PostConfigureOpenIdConnectClaimsRequestOptions(IOptionsMonitor<ClaimsRequestMapOptions> claimsRequestMapOptionsMonitor, ILoggerFactory loggerFactory)
		{
			this.ClaimsRequestMapOptionsMonitor = claimsRequestMapOptionsMonitor ?? throw new ArgumentNullException(nameof(claimsRequestMapOptionsMonitor));
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		protected internal virtual IOptionsMonitor<ClaimsRequestMapOptions> ClaimsRequestMapOptionsMonitor { get; }
		protected internal virtual ILogger Logger { get; }

		#endregion

		#region Methods

		protected internal virtual ClaimsRequest GetClaimsRequest(IDictionary<string, ClaimsRequest> claimsRequestMap, string name)
		{
			if(claimsRequestMap == null)
				throw new ArgumentNullException(nameof(claimsRequestMap));

			if(name == null)
				throw new ArgumentNullException(nameof(name));

			// ReSharper disable InvertIf
			if(claimsRequestMap.TryGetValue(name, out var claimsRequest))
			{
				var idToken = claimsRequest.IdToken ?? new Dictionary<string, ClaimsRequestItem>();
				var userInfo = claimsRequest.UserInfo ?? new Dictionary<string, ClaimsRequestItem>();

				if(idToken.Any() || userInfo.Any())
					return claimsRequest;
			}
			// ReSharper restore InvertIf

			return null;
		}

		public virtual void PostConfigure(string name, OpenIdConnectOptions options)
		{
			this.Logger.LogDebugIfEnabled($"Post-configuration of claims-request for open-id-connect-options \"{name}\" starting...");

			if(options == null)
				throw new ArgumentNullException(nameof(options));

			var claimsRequestMapOptions = this.ClaimsRequestMapOptionsMonitor.CurrentValue;
			var claimsRequestMap = claimsRequestMapOptions.ToDictionary();

			var claimsRequest = this.GetClaimsRequest(claimsRequestMap, name);

			if(claimsRequest == null)
			{
				this.Logger.LogDebugIfEnabled($"There is no claims-request configured for open-id-connect-options \"{name}\".");
				return;
			}

			var claimsRequestJson = claimsRequest.ToJson();
			this.Logger.LogDebugIfEnabled($"Claims-request for open-id-connect-options \"{name}\" is \"{claimsRequestJson}\".");

			var onRedirectToIdentityProvider = options.Events.OnRedirectToIdentityProvider;

			options.Events.OnRedirectToIdentityProvider = async context =>
			{
				if(onRedirectToIdentityProvider != null)
					await onRedirectToIdentityProvider(context);

				this.Logger.LogDebugIfEnabled($"Setting open-id-connect-message claims-parameter for \"{name}\" to \"{claimsRequestJson}\".");

				context.ProtocolMessage.Parameters.Add("claims", claimsRequestJson);
			};
		}

		#endregion
	}
}