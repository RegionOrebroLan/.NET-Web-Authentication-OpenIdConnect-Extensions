using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Logging.Extensions;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration.Extensions;

namespace RegionOrebroLan.Web.Authentication.OpenIdConnect.Events
{
	/// <summary>
	/// Events for handling claims-request.
	/// </summary>
	/// <inheritdoc />
	public class ClaimsRequestEvents : OpenIdConnectEvents
	{
		#region Constructors

		public ClaimsRequestEvents(IOptionsMonitor<ClaimsRequestMapOptions> claimsRequestMapOptionsMonitor, ILoggerFactory loggerFactory)
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

		public override async Task RedirectToIdentityProvider(RedirectContext context)
		{
			this.Logger.LogDebugIfEnabled("Redirect to IdentityProvider to handle claims-request starting...");

			if(context == null)
				throw new ArgumentNullException(nameof(context));

			await Task.CompletedTask.ConfigureAwait(false);

			var authenticationScheme = context.Scheme.Name;

			var claimsRequestMapOptions = this.ClaimsRequestMapOptionsMonitor.CurrentValue;

			if(!claimsRequestMapOptions.TryGetClaimsRequestJson(authenticationScheme, out var claimsRequestJson, this.Logger))
				return;

			this.Logger.LogDebugIfEnabled($"Setting open-id-connect-message claims-parameter for \"{authenticationScheme}\" to \"{claimsRequestJson}\".");

			context.ProtocolMessage.Parameters.Add("claims", claimsRequestJson);
		}

		#endregion
	}
}