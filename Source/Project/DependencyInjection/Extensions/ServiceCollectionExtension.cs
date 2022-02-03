using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.Events;

namespace RegionOrebroLan.Web.Authentication.OpenIdConnect.DependencyInjection.Extensions
{
	public static class ServiceCollectionExtension
	{
		#region Methods

		public static IServiceCollection AddOpenIdConnectClaimsRequest(this IServiceCollection services, IConfiguration configuration, string openIdClaimsRequestSectionPath = ConfigurationKeys.OpenIdClaimsRequestPath)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			services.Configure<ClaimsRequestMapOptions>(configuration.GetSection(openIdClaimsRequestSectionPath));

			services.AddSingleton<IPostConfigureOptions<OpenIdConnectOptions>, PostConfigureOpenIdConnectClaimsRequestOptions>();

			return services;
		}

		public static IServiceCollection AddOpenIdConnectExtensions(this IServiceCollection services, IConfiguration configuration, string openIdClaimsRequestSectionPath = ConfigurationKeys.OpenIdClaimsRequestPath)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			services.AddOpenIdConnectClaimsRequest(configuration, openIdClaimsRequestSectionPath);

			services.TryAddSingleton<ISystemClock, SystemClock>();

			services.TryAddTransient<ClaimsRequestEvents>();
			services.TryAddTransient<CountyClientSecretJwtAuthenticationEvents>();

			return services;
		}

		#endregion
	}
}