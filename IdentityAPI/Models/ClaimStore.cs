using System.Security.Claims;

namespace IdentityAPI.Models
{
	public static class ClaimStore
	{
		public static List<Claim> AllClaims = new List<Claim>()
		{
			new Claim("CreateRole","CreateRole"),
			new Claim("EditRole","EditRole"),
			new Claim("DeleteRole","DeleteRole")
		};

	}
}
