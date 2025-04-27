using System.ComponentModel.DataAnnotations;

namespace IdentityAPI.Models
{
	public class EditRole
	{
        public EditRole()
        {
            Users = new List<string>();   
        }
        public string Id {  get; set; }
        [Required(ErrorMessage ="RoleName is Required")]
        public string RoleName { get; set; }    

        public List<string> Users { get; set; } 
    }
}
