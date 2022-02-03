using System;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Logging.Extensions;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration.Extensions;

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

		public virtual void PostConfigure(string name, OpenIdConnectOptions options)
		{
			this.Logger.LogDebugIfEnabled($"Post-configuration of claims-request for open-id-connect-options \"{name}\" starting...");

			if(name == null)
				throw new ArgumentNullException(nameof(name));

			if(options == null)
				throw new ArgumentNullException(nameof(options));

			var claimsRequestMapOptions = this.ClaimsRequestMapOptionsMonitor.CurrentValue;

			if(!claimsRequestMapOptions.TryGetClaimsRequestJson(name, out var claimsRequestJson, this.Logger))
				return;

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