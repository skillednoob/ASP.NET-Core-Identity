namespace IdentityAPI.Models
{
	public class UserClaims
	{
        public UserClaims()
        {
            Claims = new List<UserClaim>();     
        }
        public string UserId { get; set; }
        public List<UserClaim> Claims { get; set; }
    }
}
