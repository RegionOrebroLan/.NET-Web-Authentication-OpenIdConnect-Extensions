using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Web.Authentication.OpenIdConnect;

namespace IntegrationTests
{
	[TestClass]
	public class ClaimsRequestTest
	{
		#region Methods

		[TestMethod]
		public async Task JsonDeserialize_Test()
		{
			await Task.CompletedTask;

			var claimRequest = JsonSerializer.Deserialize<ClaimsRequest>("{}");
			Assert.IsNotNull(claimRequest);
			Assert.IsNull(claimRequest.IdToken);
			Assert.IsNull(claimRequest.UserInfo);

			claimRequest = JsonSerializer.Deserialize<ClaimsRequest>("{\"id_token\":null}");
			Assert.IsNotNull(claimRequest);
			Assert.IsNull(claimRequest.IdToken);
			Assert.IsNull(claimRequest.UserInfo);

			claimRequest = JsonSerializer.Deserialize<ClaimsRequest>("{\"id_token\":null,\"userinfo\":null}");
			Assert.IsNotNull(claimRequest);
			Assert.IsNull(claimRequest.IdToken);
			Assert.IsNull(claimRequest.UserInfo);

			claimRequest = JsonSerializer.Deserialize<ClaimsRequest>("{\"id_token\":{}}");
			Assert.IsNotNull(claimRequest);
			Assert.IsNotNull(claimRequest.IdToken);
			Assert.IsNull(claimRequest.UserInfo);

			claimRequest = JsonSerializer.Deserialize<ClaimsRequest>("{\"id_token\":{},\"userinfo\":{}}");
			Assert.IsNotNull(claimRequest);
			Assert.IsNotNull(claimRequest.IdToken);
			Assert.IsNotNull(claimRequest.UserInfo);
		}

		[TestMethod]
		public async Task ToJson_Test()
		{
			await Task.CompletedTask;

			var claimRequest = new ClaimsRequest();

			Assert.AreEqual("{}", claimRequest.ToJson());

			claimRequest.IdToken = new Dictionary<string, ClaimsRequestItem>(StringComparer.OrdinalIgnoreCase);

			Assert.AreEqual("{\"id_token\":{}}", claimRequest.ToJson());
		}

		[TestMethod]
		public async Task ToString_Test()
		{
			await Task.CompletedTask;

			var claimRequest = new ClaimsRequest();

			Assert.AreEqual("{}", claimRequest.ToString());

			claimRequest.IdToken = new Dictionary<string, ClaimsRequestItem>(StringComparer.OrdinalIgnoreCase);

			Assert.AreEqual("{\"id_token\":{}}", claimRequest.ToString());
		}

		#endregion
	}
}