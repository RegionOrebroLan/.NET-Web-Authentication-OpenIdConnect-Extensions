using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration;

namespace IntegrationTests.Configuration
{
	[TestClass]
	public class ClaimsRequestOptionsTest
	{
		#region Methods

		[TestMethod]
		public async Task Test()
		{
			await Task.CompletedTask;

			var configuration = Global.CreateConfiguration(Path.Combine("Configuration", "Resources", "ClaimsRequestTest.json"));

			var claimsRequestOptions = new ClaimsRequestOptions();
			configuration.GetSection("ClaimsRequest-1").Bind(claimsRequestOptions);
			Assert.IsNotNull(claimsRequestOptions);
			Assert.IsNotNull(claimsRequestOptions.IdToken);
			Assert.AreEqual(4, claimsRequestOptions.IdToken.Count);
			Assert.IsNull(claimsRequestOptions.IdToken[0].Essential);
			Assert.AreEqual("key-1", claimsRequestOptions.IdToken[0].Key);
			Assert.IsNull(claimsRequestOptions.IdToken[0].Value);
			Assert.IsNull(claimsRequestOptions.IdToken[0].Values);
			Assert.IsNotNull(claimsRequestOptions.IdToken[1].Essential);
			Assert.IsTrue(claimsRequestOptions.IdToken[1].Essential.Value);
			Assert.AreEqual("key-2", claimsRequestOptions.IdToken[1].Key);
			Assert.IsNull(claimsRequestOptions.IdToken[1].Value);
			Assert.IsNull(claimsRequestOptions.IdToken[1].Values);
			Assert.IsNull(claimsRequestOptions.IdToken[2].Essential);
			Assert.AreEqual("key-3", claimsRequestOptions.IdToken[2].Key);
			Assert.AreEqual("value-3", claimsRequestOptions.IdToken[2].Value);
			Assert.IsNull(claimsRequestOptions.IdToken[2].Values);
			Assert.IsNull(claimsRequestOptions.IdToken[3].Essential);
			Assert.AreEqual("key-4", claimsRequestOptions.IdToken[3].Key);
			Assert.IsNull(claimsRequestOptions.IdToken[3].Value);
			Assert.IsNotNull(claimsRequestOptions.IdToken[3].Values);
			Assert.AreEqual(3, claimsRequestOptions.IdToken[3].Values.Length);
			Assert.IsNotNull(claimsRequestOptions.UserInfo);
			Assert.AreEqual(1, claimsRequestOptions.UserInfo.Count);
			Assert.IsNull(claimsRequestOptions.UserInfo[0].Essential);
			Assert.AreEqual("key-1", claimsRequestOptions.UserInfo[0].Key);
			Assert.IsNull(claimsRequestOptions.UserInfo[0].Value);
			Assert.IsNull(claimsRequestOptions.UserInfo[0].Values);
			Assert.AreEqual("{\"id_token\":{\"key-1\":null,\"key-2\":{\"essential\":true},\"key-3\":{\"value\":\"value-3\"},\"key-4\":{\"values\":[\"value-4-1\",\"value-4-2\",\"value-4-3\"]}},\"userinfo\":{\"key-1\":null}}", claimsRequestOptions.ToClaimsRequest().ToJson());

			claimsRequestOptions = new ClaimsRequestOptions();
			configuration.GetSection("ClaimsRequest-2").Bind(claimsRequestOptions);
			Assert.IsNotNull(claimsRequestOptions);
			Assert.IsNotNull(claimsRequestOptions.IdToken);
			Assert.AreEqual(1, claimsRequestOptions.IdToken.Count);
			Assert.IsNull(claimsRequestOptions.IdToken[0].Essential);
			Assert.AreEqual("key-1", claimsRequestOptions.IdToken[0].Key);
			Assert.IsNull(claimsRequestOptions.IdToken[0].Value);
			Assert.IsNull(claimsRequestOptions.IdToken[0].Values);
			Assert.IsNotNull(claimsRequestOptions.UserInfo);
			Assert.IsFalse(claimsRequestOptions.UserInfo.Any());
			Assert.AreEqual("{\"id_token\":{\"key-1\":null}}", claimsRequestOptions.ToClaimsRequest().ToJson());

			claimsRequestOptions = new ClaimsRequestOptions();
			configuration.GetSection("ClaimsRequest-3").Bind(claimsRequestOptions);
			Assert.IsNotNull(claimsRequestOptions);
			Assert.IsNotNull(claimsRequestOptions.IdToken);
			Assert.IsNotNull(claimsRequestOptions.UserInfo);
			Assert.AreEqual("{}", claimsRequestOptions.ToClaimsRequest().ToJson());

			claimsRequestOptions = new ClaimsRequestOptions();
			configuration.GetSection("ClaimsRequest-4").Bind(claimsRequestOptions);
			Assert.IsNotNull(claimsRequestOptions);
			Assert.IsNotNull(claimsRequestOptions.IdToken);
			Assert.IsNotNull(claimsRequestOptions.UserInfo);
			Assert.AreEqual("{}", claimsRequestOptions.ToClaimsRequest().ToJson());
		}

		#endregion
	}
}