using System;
using System.Collections.Generic;
using System.Linq;

namespace RegionOrebroLan.Web.Authentication.OpenIdConnect.Configuration
{
	public class ClaimsRequestOptions
	{
		#region Properties

		public virtual IList<ClaimsRequestItemOptions> IdToken { get; } = new List<ClaimsRequestItemOptions>();
		public virtual IList<ClaimsRequestItemOptions> UserInfo { get; } = new List<ClaimsRequestItemOptions>();

		#endregion

		#region Methods

		protected internal virtual ClaimsRequestItem CreateClaimsRequestItem(ClaimsRequestItemOptions options)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			if(options.Essential == null && options.Value == null && options.Values == null)
				return null;

			return new ClaimsRequestItem
			{
				Essential = options.Essential,
				Value = options.Value,
				Values = options.Values
			};
		}

		public virtual ClaimsRequest ToClaimsRequest()
		{
			var claimsRequest = new ClaimsRequest();

			try
			{
				if(this.IdToken.Any())
				{
					claimsRequest.IdToken = new Dictionary<string, ClaimsRequestItem>(StringComparer.OrdinalIgnoreCase);

					foreach(var item in this.IdToken)
					{
						if(item == null)
							continue;

						claimsRequest.IdToken.Add(item.Key, this.CreateClaimsRequestItem(item));
					}
				}

				if(this.UserInfo.Any())
				{
					claimsRequest.UserInfo = new Dictionary<string, ClaimsRequestItem>(StringComparer.OrdinalIgnoreCase);

					foreach(var item in this.UserInfo)
					{
						if(item == null)
							continue;

						claimsRequest.UserInfo.Add(item.Key, this.CreateClaimsRequestItem(item));
					}
				}
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException("Could not create claims-request.", exception);
			}

			return claimsRequest;
		}

		#endregion
	}
}