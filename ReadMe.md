# .NET-Web-Authentication-OpenIdConnect-Extensions

Additions and extensions for .NET web-authentication-openidconnect (ASP.NET Core).

[![NuGet](https://img.shields.io/nuget/v/RegionOrebroLan.Web.Authentication.OpenIdConnect.svg?label=NuGet)](https://www.nuget.org/packages/RegionOrebroLan.Web.Authentication.OpenIdConnect)

## 1 Features

### 1.1 Requesting Claims using the "claims" Request Parameter

- https://openid.net/specs/openid-connect-core-1_0.html#ClaimsParameter

#### 1.1.1 Configuration

- [Configuration-example](/Source/Tests/Integration-tests/DependencyInjection/Extensions/Resources/ServiceCollectionExtensionTest.json)

#### 1.1.2 Code

	using System;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using RegionOrebroLan.Web.Authentication.OpenIdConnect.DependencyInjection.Extensions;

	namespace Application
	{
		public class Startup
		{
			#region Constructors

			public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
			{
				this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
				this.HostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
			}

			#endregion

			#region Properties

			protected internal virtual IConfiguration Configuration { get; }
			protected internal virtual IHostEnvironment HostEnvironment { get; }

			#endregion

			#region Methods

			public virtual void Configure(IApplicationBuilder applicationBuilder) { }

			public virtual void ConfigureServices(IServiceCollection services)
			{
				if(services == null)
					throw new ArgumentNullException(nameof(services));

				services.AddOpenIdConnectClaimsRequest(this.Configuration);
			}

			#endregion
		}
	}