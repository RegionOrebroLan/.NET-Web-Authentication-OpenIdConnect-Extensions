using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration;

namespace RegionOrebroLan.Web.Authentication.OpenIdConnect.Events
{
	/// <summary>
	/// Events for handling client_secret_jwt authentication with County IdP.
	/// </summary>
	/// <inheritdoc />
	public class CountyClientSecretJwtAuthenticationEvents : ClaimsRequestEvents
	{
		#region Fields

		private JwtSecurityTokenHandler _jwtSecurityTokenHandler;

		#endregion

		#region Constructors

		public CountyClientSecretJwtAuthenticationEvents(IOptionsMonitor<ClaimsRequestMapOptions> claimsRequestMapOptionsMonitor, ILoggerFactory loggerFactory, ISystemClock systemClock) : base(claimsRequestMapOptionsMonitor, loggerFactory)
		{
			this.SystemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		}

		#endregion

		#region Properties

		protected internal virtual JwtSecurityTokenHandler JwtSecurityTokenHandler
		{
			get
			{
				// ReSharper disable InvertIf
				if(this._jwtSecurityTokenHandler == null)
				{
					var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

					jwtSecurityTokenHandler.OutboundClaimTypeMap.Clear();

					this._jwtSecurityTokenHandler = jwtSecurityTokenHandler;
				}
				// ReSharper restore InvertIf

				return this._jwtSecurityTokenHandler;
			}
		}

		protected internal virtual DateTime Now => this.SystemClock.UtcNow.UtcDateTime;
		protected internal virtual string SigningAlgorithm => SecurityAlgorithms.HmacSha256;
		protected internal virtual ISystemClock SystemClock { get; }

		#endregion

		#region Methods

		public override async Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			var clientAssertion = await this.CreateClientAssertionAsync(context);

			context.TokenEndpointRequest.ClientAssertion = clientAssertion.Value;
			context.TokenEndpointRequest.ClientAssertionType = clientAssertion.Type;

			context.TokenEndpointRequest.ClientId = null;
			context.TokenEndpointRequest.ClientSecret = null;
		}

		protected internal virtual async Task<ClientAssertion> CreateClientAssertionAsync(AuthorizationCodeReceivedContext authorizationCodeReceivedContext)
		{
			if(authorizationCodeReceivedContext == null)
				throw new ArgumentNullException(nameof(authorizationCodeReceivedContext));

			var now = this.Now;

			var audience = authorizationCodeReceivedContext.TokenEndpointRequest.TokenEndpoint ?? await this.GetTokenEndpointAsync(authorizationCodeReceivedContext.HttpContext, authorizationCodeReceivedContext.Options);

			var claims = new List<Claim>();

			var signingCredentials = this.CreateSigningCredentials(authorizationCodeReceivedContext.ProtocolMessage, authorizationCodeReceivedContext.Options);

			var jwtSecurityToken = this.CreateJwtSecurityToken(audience, claims, authorizationCodeReceivedContext.ProtocolMessage, now, authorizationCodeReceivedContext.Options, signingCredentials);

			var clientAssertion = new ClientAssertion
			{
				Type = OidcConstants.ClientAssertionTypes.JwtBearer,
				Value = this.JwtSecurityTokenHandler.WriteToken(jwtSecurityToken)
			};

			return clientAssertion;
		}

		protected internal virtual JwtSecurityToken CreateJwtSecurityToken(string audience, IList<Claim> claims, OpenIdConnectMessage message, DateTime now, OpenIdConnectOptions options, SigningCredentials signingCredentials)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			this.EnsureRequiredClaims(claims, now, options);

			var jwtSecurityToken = new JwtSecurityToken(options.ClientId, audience, claims, now, now.AddMinutes(1), signingCredentials);

			return jwtSecurityToken;
		}

		protected internal virtual SecurityKey CreateSecurityKey(OpenIdConnectMessage message, OpenIdConnectOptions options)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			var bytes = Encoding.UTF8.GetBytes(options.ClientSecret);

			var symmetricSecurityKey = new SymmetricSecurityKey(bytes);

			return symmetricSecurityKey;
		}

		protected internal virtual SigningCredentials CreateSigningCredentials(OpenIdConnectMessage message, OpenIdConnectOptions options)
		{
			var securityKey = this.CreateSecurityKey(message, options);

			var signingCredentials = new SigningCredentials(securityKey, this.SigningAlgorithm);

			return signingCredentials;
		}

		protected internal virtual void EnsureRequiredClaims(IList<Claim> claims, DateTime now, OpenIdConnectOptions options)
		{
			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			if(options == null)
				throw new ArgumentNullException(nameof(options));

			if(!claims.Any(claim => string.Equals(JwtClaimTypes.IssuedAt, claim.Type, StringComparison.OrdinalIgnoreCase)))
				claims.Add(new(JwtClaimTypes.IssuedAt, now.ToEpochTime().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64));

			if(!claims.Any(claim => string.Equals(JwtClaimTypes.JwtId, claim.Type, StringComparison.OrdinalIgnoreCase)))
				claims.Add(new(JwtClaimTypes.JwtId, Guid.NewGuid().ToString()));

			if(!claims.Any(claim => string.Equals(JwtClaimTypes.Subject, claim.Type, StringComparison.OrdinalIgnoreCase)))
				claims.Add(new(JwtClaimTypes.Subject, options.ClientId));
		}

		protected internal virtual async Task<string> GetTokenEndpointAsync(HttpContext httpContext, OpenIdConnectOptions options)
		{
			if(httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			if(options == null)
				throw new ArgumentNullException(nameof(options));

			var configuration = await options.ConfigurationManager.GetConfigurationAsync(httpContext.RequestAborted);

			return configuration.TokenEndpoint;
		}

		#endregion
	}
}