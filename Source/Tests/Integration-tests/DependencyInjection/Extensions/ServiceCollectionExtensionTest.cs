using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.DependencyInjection.Extensions;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.Events;

namespace IntegrationTests.DependencyInjection.Extensions
{
	[TestClass]
	public class ServiceCollectionExtensionTest
	{
		#region Methods

		[TestMethod]
		public async Task AddOpenIdConnectClaimsRequest_Test()
		{
			const string authenticationSchemeParameterName = "AuthenticationScheme";

			// Key is authentication-scheme and value is the number of expected parameters added.
			var testDictionary = new Dictionary<string, int>
			{
				{ "authentication-scheme-1", 2 },
				{ "authentication-scheme-2", 2 },
				{ "authentication-scheme-3", 1 },
				{ "authentication-scheme-4", 1 }
			};

			var configuration = Global.CreateConfiguration("appsettings.json", Path.Combine("DependencyInjection", "Extensions", "Resources", "ServiceCollectionExtensionTest.json"));

			var services = Global.CreateServices(configuration);

			services.AddOpenIdConnectClaimsRequest(configuration);

			foreach(var authenticationSchemeName in testDictionary.Keys)
			{
				services.Configure<OpenIdConnectOptions>(authenticationSchemeName, options =>
				{
					options.Authority = $"https://{authenticationSchemeName}";
					options.Events.OnRedirectToIdentityProvider = async context =>
					{
						await Task.CompletedTask;

						context.ProtocolMessage.Parameters.Add(authenticationSchemeParameterName, authenticationSchemeName);
					};
				});
			}

			await using(var serviceProvider = services.BuildServiceProvider())
			{
				var openIdConnectOptionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>();
				var claimsRequestMap = serviceProvider.GetRequiredService<IOptionsMonitor<ClaimsRequestMapOptions>>().CurrentValue.ToDictionary();
				Assert.AreEqual(3, claimsRequestMap.Count);

				foreach(var (authenticationSchemeName, expectedNumberOfParameters) in testDictionary)
				{
					var openIdConnectOptions = openIdConnectOptionsMonitor.Get(authenticationSchemeName);
					var redirectContext = await this.CreateRedirectContextAsync(openIdConnectOptions);
					Assert.AreEqual(0, redirectContext.ProtocolMessage.Parameters.Count);
					await openIdConnectOptions.Events.OnRedirectToIdentityProvider(redirectContext);
					Assert.AreEqual(expectedNumberOfParameters, redirectContext.ProtocolMessage.Parameters.Count);
					Assert.AreEqual(authenticationSchemeName, redirectContext.ProtocolMessage.Parameters[authenticationSchemeParameterName]);
					Assert.AreEqual($"https://{authenticationSchemeName}", redirectContext.Options.Authority);

					if(expectedNumberOfParameters > 1)
					{
						Assert.AreEqual(claimsRequestMap[authenticationSchemeName].ToJson(), redirectContext.ProtocolMessage.Parameters["claims"]);
					}
				}
			}
		}

		[TestMethod]
		public async Task AddOpenIdConnectExtensions_Test()
		{
			var configuration = Global.CreateDefaultConfigurationBuilder().Build();
			var services = Global.CreateServices(configuration);

			services.AddOpenIdConnectExtensions(configuration);

			await using(var serviceProvider = services.BuildServiceProvider())
			{
				var claimsRequestMapOptions = serviceProvider.GetRequiredService<IOptionsMonitor<ClaimsRequestMapOptions>>().CurrentValue;
				Assert.IsNotNull(claimsRequestMapOptions);

				var claimsRequestEvents = serviceProvider.GetRequiredService<ClaimsRequestEvents>();
				Assert.IsNotNull(claimsRequestEvents);

				var countyClientSecretJwtAuthenticationEvents = serviceProvider.GetRequiredService<CountyClientSecretJwtAuthenticationEvents>();
				Assert.IsNotNull(countyClientSecretJwtAuthenticationEvents);
			}
		}

		protected internal virtual async Task<RedirectContext> CreateRedirectContextAsync(OpenIdConnectOptions openIdConnectOptions)
		{
			var authenticationProperties = new AuthenticationProperties();
			var authenticationScheme = new AuthenticationScheme("Test", null, Mock.Of<IAuthenticationHandler>().GetType());
			var httpContext = new DefaultHttpContext();

			var redirectContext = new RedirectContext(httpContext, authenticationScheme, openIdConnectOptions, authenticationProperties)
			{
				ProtocolMessage = new OpenIdConnectMessage()
			};

			return await Task.FromResult(redirectContext);
		}

		#endregion
	}
}