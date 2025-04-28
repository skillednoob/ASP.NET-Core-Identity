using System.ComponentModel.DataAnnotations;

namespace IdentityAPI.Models
{
	public class EditUser
	{
        public EditUser()
        {
            Claims=new List<string>();
            Roles=new List<string>();
        }
        public string Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string City { get; set; }
        public List<string> Claims { get; set; }
        public List<string> Roles { get; set; }
    }
}
