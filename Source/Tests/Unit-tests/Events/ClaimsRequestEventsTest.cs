using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.Events;
using TestHelpers.Mocks.Logging;

namespace UnitTests.Events
{
	[TestClass]
	public class ClaimsRequestEventsTest
	{
		#region Methods

		protected internal virtual async Task<ClaimsRequestEvents> CreateClaimsRequestEventsAsync(ClaimsRequestMapOptions claimsRequestMapOptions)
		{
			return await this.CreateClaimsRequestEventsAsync(claimsRequestMapOptions, Mock.Of<ILoggerFactory>());
		}

		protected internal virtual async Task<ClaimsRequestEvents> CreateClaimsRequestEventsAsync(ClaimsRequestMapOptions claimsRequestMapOptions, ILoggerFactory loggerFactory)
		{
			var optionsMonitorMock = new Mock<IOptionsMonitor<ClaimsRequestMapOptions>>();
			optionsMonitorMock.Setup(optionsMonitor => optionsMonitor.CurrentValue).Returns(claimsRequestMapOptions);
			var optionsMonitor = optionsMonitorMock.Object;

			return await this.CreateClaimsRequestEventsAsync(optionsMonitor, loggerFactory);
		}

		protected internal virtual async Task<ClaimsRequestEvents> CreateClaimsRequestEventsAsync(IOptionsMonitor<ClaimsRequestMapOptions> claimsRequestMapOptionsMonitor, ILoggerFactory loggerFactory)
		{
			return await Task.FromResult(new ClaimsRequestEvents(claimsRequestMapOptionsMonitor, loggerFactory));
		}

		protected internal virtual async Task<RedirectContext> CreateRedirectContextAsync(string authenticationScheme)
		{
			return await this.CreateRedirectContextAsync(authenticationScheme, Mock.Of<HttpContext>(), Mock.Of<OpenIdConnectOptions>(), Mock.Of<AuthenticationProperties>());
		}

		protected internal virtual async Task<RedirectContext> CreateRedirectContextAsync(string authenticationScheme, HttpContext httpContext, OpenIdConnectOptions options, AuthenticationProperties properties)
		{
			var redirectContext = new RedirectContext(httpContext, new AuthenticationScheme(authenticationScheme, null, typeof(OpenIdConnectHandler)), options, properties)
			{
				ProtocolMessage = new OpenIdConnectMessage()
			};

			return await Task.FromResult(redirectContext);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task RedirectToIdentityProvider_IfTheContextParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			using(var loggerFactoryMock = Global.CreateLoggerFactoryMock())
			{
				var claimsRequestEvents = await this.CreateClaimsRequestEventsAsync(new ClaimsRequestMapOptions(), loggerFactoryMock);

				try
				{
					await claimsRequestEvents.RedirectToIdentityProvider(null);
				}
				catch(ArgumentNullException argumentNullException)
				{
					if(string.Equals(argumentNullException.ParamName, "context", StringComparison.Ordinal))
						throw;
				}
			}
		}

		[TestMethod]
		public async Task RedirectToIdentityProvider_IfThereAreClaimsRequestMappings_ShouldDebugLog()
		{
			const string authenticationScheme = "Test";
			const string claimsRequestJson = "{\"id_token\":{\"Key-1\":null}}";

			using(var loggerFactoryMock = Global.CreateLoggerFactoryMock())
			{
				loggerFactoryMock.EnabledMode = LogLevelEnabledMode.Enabled;

				var claimsRequestMappingOptions = new ClaimsRequestMappingOptions
				{
					AuthenticationScheme = authenticationScheme,
					ClaimsRequest = new ClaimsRequestOptions
					{
						IdToken =
						{
							new ClaimsRequestItemOptions
							{
								Key = "Key-1"
							}
						}
					}
				};
				var claimsRequestMapOptions = new ClaimsRequestMapOptions();
				claimsRequestMapOptions.Mappings.Add(claimsRequestMappingOptions);
				var claimsRequestEvents = await this.CreateClaimsRequestEventsAsync(claimsRequestMapOptions, loggerFactoryMock);
				var redirectContext = await this.CreateRedirectContextAsync(authenticationScheme);
				await claimsRequestEvents.RedirectToIdentityProvider(redirectContext);
				Assert.AreEqual(3, loggerFactoryMock.Logs.Count());
				Assert.AreEqual("Redirect to IdentityProvider to handle claims-request starting...", loggerFactoryMock.Logs.ElementAt(0).Message);
				Assert.AreEqual($"Claims-request for open-id-connect-options \"{authenticationScheme}\" is \"{claimsRequestJson}\".", loggerFactoryMock.Logs.ElementAt(1).Message);
				Assert.AreEqual($"Setting open-id-connect-message claims-parameter for \"{authenticationScheme}\" to \"{claimsRequestJson}\".", loggerFactoryMock.Logs.ElementAt(2).Message);
			}
		}

		[TestMethod]
		public async Task RedirectToIdentityProvider_IfThereAreNoClaimsRequestMappings_ShouldDebugLog()
		{
			const string authenticationScheme = "Test";

			using(var loggerFactoryMock = Global.CreateLoggerFactoryMock())
			{
				loggerFactoryMock.EnabledMode = LogLevelEnabledMode.Enabled;

				var claimsRequestEvents = await this.CreateClaimsRequestEventsAsync(new ClaimsRequestMapOptions(), loggerFactoryMock);
				var redirectContext = await this.CreateRedirectContextAsync(authenticationScheme);
				await claimsRequestEvents.RedirectToIdentityProvider(redirectContext);
				Assert.AreEqual(2, loggerFactoryMock.Logs.Count());
				Assert.AreEqual("Redirect to IdentityProvider to handle claims-request starting...", loggerFactoryMock.Logs.ElementAt(0).Message);
				Assert.AreEqual($"There is no claims-request configured for open-id-connect-options \"{authenticationScheme}\".", loggerFactoryMock.Logs.ElementAt(1).Message);
			}
		}

		[TestMethod]
		public async Task RedirectToIdentityProvider_Test()
		{
			const string authenticationScheme = "Test";
			const string claimsRequestJson = "{\"id_token\":{\"Key-1\":null}}";

			using(var loggerFactoryMock = Global.CreateLoggerFactoryMock())
			{
				var claimsRequestMappingOptions = new ClaimsRequestMappingOptions
				{
					AuthenticationScheme = authenticationScheme,
					ClaimsRequest = new ClaimsRequestOptions
					{
						IdToken =
						{
							new ClaimsRequestItemOptions
							{
								Key = "Key-1"
							}
						}
					}
				};
				var claimsRequestMapOptions = new ClaimsRequestMapOptions();
				claimsRequestMapOptions.Mappings.Add(claimsRequestMappingOptions);
				var claimsRequestEvents = await this.CreateClaimsRequestEventsAsync(claimsRequestMapOptions, loggerFactoryMock);
				var redirectContext = await this.CreateRedirectContextAsync(authenticationScheme);
				await claimsRequestEvents.RedirectToIdentityProvider(redirectContext);
				Assert.AreEqual(1, redirectContext.ProtocolMessage.Parameters.Count);
				var firstParameter = redirectContext.ProtocolMessage.Parameters.ElementAt(0);
				Assert.AreEqual("claims", firstParameter.Key);
				Assert.AreEqual(claimsRequestJson, firstParameter.Value);
			}
		}

		#endregion
	}
}