using IdentityAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AdministrationController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _roleManager;
        public AdministrationController(RoleManager<IdentityRole> roleManager)
        {
				_roleManager = roleManager;
        }

		[HttpPost("CreateRole")]
		public async Task<IActionResult> CreateRole(CreateRole model)
		{
			if (ModelState.IsValid)
			{
				IdentityRole identity = new IdentityRole { Name = model.RoleName };

				IdentityResult result = await _roleManager.CreateAsync(identity);
				if (result.Succeeded)
				{
					return Ok(new { message = "Role Added Succesfully" });
				}
				foreach(IdentityError error in result.Errors)
				{
					ModelState.AddModelError(" ", error.Description);
				}
			}
			return BadRequest(model);
		}


		[HttpGet("ListRoles")]
		public IActionResult ListRoles()
		{
			var roles=_roleManager.Roles;
			return Ok(roles);
		}


		[HttpPost("EditRole")]
		public async Task<IActionResult> EditRole(EditRole model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			else
			{
				var role=await _roleManager.FindByIdAsync(model.Id);
				if(role == null)
				{
					return NotFound(new { Message = $"Role with id={model.Id} cannot be found " });
				}
				else
				{
					role.Name=model.RoleName;
					var result=await _roleManager.UpdateAsync(role);
					if(result.Succeeded)
					{
						return Ok(new { Message = "Role Updated Succesfully" });
					}
					var errors=result.Errors.Select(e=>e.Description);
					return BadRequest(errors);
				}
			}
		}
    }
}
