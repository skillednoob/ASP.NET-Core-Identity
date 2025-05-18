namespace IdentityAPI.Models
{
	public class RefreshToken
	{
		public int Id { get; set; }
		public string Token { get; set; }
		public string UserId { get; set; }
		public ApplicationUser User { get; set; }
		public DateTime Expires { get; set; }
		public DateTime Created { get; set; }
		public DateTime? Revoked { get; set; }


		public bool IsUsed { get; set; } = false;  // ✅ Mark if token was already used
		public bool IsRevoked { get; set; } = false;

		public bool IsExpired => DateTime.UtcNow >= Expires;
		public bool IsActive => Revoked == null && !IsExpired;
	}
}
//ACTUALLY A TABLE
/*  
 *   1st create this model
 *   2>add in applicationdb context class(dbset line)
 *   3>add-migration
 *   4>update-database
 *  */