using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.Events;

namespace UnitTests.Events
{
	[TestClass]
	public class CountyClientSecretJwtAuthenticationEventsTest
	{
		#region Fields

		private const string _tokenEndpoint = "https://localhost/connect/token";

		#endregion

		#region Properties

		protected internal virtual string TokenEndpoint => _tokenEndpoint;

		#endregion

		#region Methods

		[TestMethod]
		public async Task AuthorizationCodeReceived_Test()
		{
			const string authenticationScheme = "Test";
			const string clientId = "Client-1";
			const string clientSecret = "Client-secret-01"; // Minimum length is 16.
			var systemClock = await this.CreateSystemClockAsync(); // 2000-08-08 08:00:00 +00:00

			using(var loggerFactoryMock = Global.CreateLoggerFactoryMock())
			{
				var countyClientSecretJwtAuthenticationEvents = await this.CreateCountyClientSecretJwtAuthenticationEventsAsync(new ClaimsRequestMapOptions(), loggerFactoryMock, systemClock);
				var authorizationCodeReceivedContext = await this.CreateAuthorizationCodeReceivedContextAsync(authenticationScheme);
				authorizationCodeReceivedContext.Options.ClientId = clientId;
				authorizationCodeReceivedContext.Options.ClientSecret = clientSecret;
				await countyClientSecretJwtAuthenticationEvents.AuthorizationCodeReceived(authorizationCodeReceivedContext);

				Assert.AreEqual(OidcConstants.ClientAssertionTypes.JwtBearer, authorizationCodeReceivedContext.TokenEndpointRequest?.ClientAssertionType);

				var jwtSecurityTokenHandler = new JwtSecurityTokenHandler
				{
					MapInboundClaims = false
				};
				var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clientSecret));
				var tokenValidationParameters = new TokenValidationParameters
				{
					IssuerSigningKey = securityKey,
					LifetimeValidator = (notBefore, expires, _, _) =>
					{
						var utcNow = systemClock.UtcNow.UtcDateTime;

						return notBefore <= utcNow && expires >= utcNow;
					},
					ValidateAudience = true,
					ValidateIssuer = true,
					ValidateIssuerSigningKey = true,
					ValidateLifetime = true,
					ValidAudience = this.TokenEndpoint,
					ValidIssuer = clientId
				};

				var claimsPrincipal = jwtSecurityTokenHandler.ValidateToken(authorizationCodeReceivedContext.TokenEndpointRequest?.ClientAssertion, tokenValidationParameters, out var securityToken);

				Assert.IsNotNull(claimsPrincipal);
				Assert.AreEqual(7, claimsPrincipal.Claims.Count());
				Assert.AreEqual(this.TokenEndpoint, claimsPrincipal.FindFirst(JwtClaimTypes.Audience)?.Value);
				Assert.AreEqual(clientId, claimsPrincipal.FindFirst(JwtClaimTypes.Issuer)?.Value);
				Assert.AreEqual(clientId, claimsPrincipal.FindFirst(JwtClaimTypes.Subject)?.Value);

				Assert.IsNotNull(securityToken);
				Assert.AreEqual(clientId, securityToken.Issuer);
				Assert.AreEqual(systemClock.UtcNow.UtcDateTime, securityToken.ValidFrom);
				Assert.AreEqual(systemClock.UtcNow.UtcDateTime.AddMinutes(1), securityToken.ValidTo);
			}
		}

		protected internal virtual async Task<AuthorizationCodeReceivedContext> CreateAuthorizationCodeReceivedContextAsync(string authenticationScheme)
		{
			return await this.CreateAuthorizationCodeReceivedContextAsync(authenticationScheme, Mock.Of<HttpContext>(), new OpenIdConnectOptions(), Mock.Of<AuthenticationProperties>());
		}

		protected internal virtual async Task<AuthorizationCodeReceivedContext> CreateAuthorizationCodeReceivedContextAsync(string authenticationScheme, HttpContext httpContext, OpenIdConnectOptions options, AuthenticationProperties properties)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			if(options.ConfigurationManager == null)
			{
				var openIdConnectConfiguration = new OpenIdConnectConfiguration
				{
					TokenEndpoint = this.TokenEndpoint
				};

				var configurationManagerMock = new Mock<IConfigurationManager<OpenIdConnectConfiguration>>();

				configurationManagerMock.Setup(configurationManager => configurationManager.GetConfigurationAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(openIdConnectConfiguration));

				options.ConfigurationManager = configurationManagerMock.Object;
			}

			var authorizationCodeReceivedContext = new AuthorizationCodeReceivedContext(httpContext, new AuthenticationScheme(authenticationScheme, null, typeof(OpenIdConnectHandler)), options, properties)
			{
				ProtocolMessage = new OpenIdConnectMessage(),
				TokenEndpointRequest = new OpenIdConnectMessage(),
				TokenEndpointResponse = new OpenIdConnectMessage()
			};

			return await Task.FromResult(authorizationCodeReceivedContext);
		}

		protected internal virtual async Task<CountyClientSecretJwtAuthenticationEvents> CreateCountyClientSecretJwtAuthenticationEventsAsync(ClaimsRequestMapOptions claimsRequestMapOptions)
		{
			return await this.CreateCountyClientSecretJwtAuthenticationEventsAsync(claimsRequestMapOptions, Mock.Of<ILoggerFactory>(), new SystemClock());
		}

		protected internal virtual async Task<CountyClientSecretJwtAuthenticationEvents> CreateCountyClientSecretJwtAuthenticationEventsAsync(ClaimsRequestMapOptions claimsRequestMapOptions, ILoggerFactory loggerFactory, ISystemClock systemClock)
		{
			var optionsMonitorMock = new Mock<IOptionsMonitor<ClaimsRequestMapOptions>>();
			optionsMonitorMock.Setup(optionsMonitor => optionsMonitor.CurrentValue).Returns(claimsRequestMapOptions);
			var optionsMonitor = optionsMonitorMock.Object;

			return await this.CreateCountyClientSecretJwtAuthenticationEventsAsync(optionsMonitor, loggerFactory, systemClock);
		}

		protected internal virtual async Task<CountyClientSecretJwtAuthenticationEvents> CreateCountyClientSecretJwtAuthenticationEventsAsync(IOptionsMonitor<ClaimsRequestMapOptions> claimsRequestMapOptionsMonitor, ILoggerFactory loggerFactory, ISystemClock systemClock)
		{
			return await Task.FromResult(new CountyClientSecretJwtAuthenticationEvents(claimsRequestMapOptionsMonitor, loggerFactory, systemClock));
		}

		protected internal virtual async Task<ISystemClock> CreateSystemClockAsync(DateTimeOffset utcNow)
		{
			var systemClockMock = new Mock<ISystemClock>();

			systemClockMock.Setup(systemClock => systemClock.UtcNow).Returns(utcNow);

			return await Task.FromResult(systemClockMock.Object);
		}

		protected internal virtual async Task<ISystemClock> CreateSystemClockAsync(int year = 2000, int month = 8, int day = 8, int hour = 8)
		{
			return await this.CreateSystemClockAsync(new DateTimeOffset(year, month, day, hour, 0, 0, 0, TimeSpan.Zero));
		}

		#endregion
	}
}