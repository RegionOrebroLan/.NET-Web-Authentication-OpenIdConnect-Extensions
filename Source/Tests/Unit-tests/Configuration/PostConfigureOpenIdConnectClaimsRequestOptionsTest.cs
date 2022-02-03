using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration;
using TestHelpers.Mocks.Logging;

namespace UnitTests.Configuration
{
	[TestClass]
	public class PostConfigureOpenIdConnectClaimsRequestOptionsTest
	{
		#region Methods

		protected internal virtual async Task<PostConfigureOpenIdConnectClaimsRequestOptions> CreatePostConfigureOpenIdConnectClaimsRequestOptionsAsync(ClaimsRequestMapOptions claimsRequestMapOptions)
		{
			return await this.CreatePostConfigureOpenIdConnectClaimsRequestOptionsAsync(claimsRequestMapOptions, Mock.Of<ILoggerFactory>());
		}

		protected internal virtual async Task<PostConfigureOpenIdConnectClaimsRequestOptions> CreatePostConfigureOpenIdConnectClaimsRequestOptionsAsync(ClaimsRequestMapOptions claimsRequestMapOptions, ILoggerFactory loggerFactory)
		{
			var optionsMonitorMock = new Mock<IOptionsMonitor<ClaimsRequestMapOptions>>();
			optionsMonitorMock.Setup(optionsMonitor => optionsMonitor.CurrentValue).Returns(claimsRequestMapOptions);
			var optionsMonitor = optionsMonitorMock.Object;

			return await this.CreatePostConfigureOpenIdConnectClaimsRequestOptionsAsync(optionsMonitor, loggerFactory);
		}

		protected internal virtual async Task<PostConfigureOpenIdConnectClaimsRequestOptions> CreatePostConfigureOpenIdConnectClaimsRequestOptionsAsync(IOptionsMonitor<ClaimsRequestMapOptions> claimsRequestMapOptionsMonitor, ILoggerFactory loggerFactory)
		{
			return await Task.FromResult(new PostConfigureOpenIdConnectClaimsRequestOptions(claimsRequestMapOptionsMonitor, loggerFactory));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task PostConfigure_IfTheNameParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			using(var loggerFactoryMock = Global.CreateLoggerFactoryMock())
			{
				var postConfigureOpenIdConnectClaimsRequestOptions = await this.CreatePostConfigureOpenIdConnectClaimsRequestOptionsAsync(new ClaimsRequestMapOptions(), loggerFactoryMock);

				try
				{
					postConfigureOpenIdConnectClaimsRequestOptions.PostConfigure(null, new OpenIdConnectOptions());
				}
				catch(ArgumentNullException argumentNullException)
				{
					if(string.Equals(argumentNullException.ParamName, "name", StringComparison.Ordinal))
						throw;
				}
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task PostConfigure_IfTheOptionsParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			using(var loggerFactoryMock = Global.CreateLoggerFactoryMock())
			{
				var postConfigureOpenIdConnectClaimsRequestOptions = await this.CreatePostConfigureOpenIdConnectClaimsRequestOptionsAsync(new ClaimsRequestMapOptions(), loggerFactoryMock);

				try
				{
					postConfigureOpenIdConnectClaimsRequestOptions.PostConfigure(string.Empty, null);
				}
				catch(ArgumentNullException argumentNullException)
				{
					if(string.Equals(argumentNullException.ParamName, "options", StringComparison.Ordinal))
						throw;
				}
			}
		}

		[TestMethod]
		public async Task PostConfigure_IfThereAreClaimsRequestMappings_ShouldDebugLog()
		{
			const string name = "Test";

			using(var loggerFactoryMock = Global.CreateLoggerFactoryMock())
			{
				loggerFactoryMock.EnabledMode = LogLevelEnabledMode.Enabled;

				var claimsRequestMappingOptions = new ClaimsRequestMappingOptions
				{
					AuthenticationScheme = name,
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
				var postConfigureOpenIdConnectClaimsRequestOptions = await this.CreatePostConfigureOpenIdConnectClaimsRequestOptionsAsync(claimsRequestMapOptions, loggerFactoryMock);
				var openIdConnectOptions = new OpenIdConnectOptions();
				postConfigureOpenIdConnectClaimsRequestOptions.PostConfigure(name, openIdConnectOptions);
				Assert.AreEqual(2, loggerFactoryMock.Logs.Count());
				Assert.AreEqual($"Post-configuration of claims-request for open-id-connect-options \"{name}\" starting...", loggerFactoryMock.Logs.ElementAt(0).Message);
				Assert.AreEqual($"Claims-request for open-id-connect-options \"{name}\" is \"{{\"id_token\":{{\"Key-1\":null}}}}\".", loggerFactoryMock.Logs.ElementAt(1).Message);
			}
		}

		[TestMethod]
		public async Task PostConfigure_IfThereAreNoClaimsRequestMappings_ShouldDebugLog()
		{
			const string name = "Test";

			using(var loggerFactoryMock = Global.CreateLoggerFactoryMock())
			{
				loggerFactoryMock.EnabledMode = LogLevelEnabledMode.Enabled;

				var postConfigureOpenIdConnectClaimsRequestOptions = await this.CreatePostConfigureOpenIdConnectClaimsRequestOptionsAsync(new ClaimsRequestMapOptions(), loggerFactoryMock);
				var openIdConnectOptions = new OpenIdConnectOptions();
				postConfigureOpenIdConnectClaimsRequestOptions.PostConfigure(name, openIdConnectOptions);
				Assert.AreEqual(2, loggerFactoryMock.Logs.Count());
				Assert.AreEqual($"Post-configuration of claims-request for open-id-connect-options \"{name}\" starting...", loggerFactoryMock.Logs.ElementAt(0).Message);
				Assert.AreEqual($"There is no claims-request configured for open-id-connect-options \"{name}\".", loggerFactoryMock.Logs.ElementAt(1).Message);
			}
		}

		#endregion
	}
}