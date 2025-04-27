using IdentityAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles ="Admin")]
	public class AdministrationController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly UserManager<ApplicationUser> _userManager;
		public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
				_roleManager = roleManager;
			_userManager = userManager;
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

		[HttpGet("ListUsersOfARole")]
		public async Task<IActionResult> ListUsersOfARole(string roleId)
		{
			var role=await _roleManager.FindByIdAsync(roleId);
			if(role == null)
			{
				return NotFound(new { Message = $"Role with id={roleId} cannot be found " });
			}
			var model=new List<UserRole>();

            foreach (var user in _userManager.Users.ToList())
            {

				var userViewModel = new UserRole
				{
					UserId = user.Id,
					UserName = user.UserName
				};
				if(await _userManager.IsInRoleAsync(user, role.Name))
				{
					userViewModel.IsSelected = true;
				}
				else
				{
					userViewModel.IsSelected=false;
				}
				model.Add(userViewModel);
            }
			return Ok(model);
        }

		[HttpPost("AddOrRemoveUsersForARole")]
		public async Task<IActionResult> EditUsersForARole(List<UserRole> model,string roleId)
		{
			var role = await _roleManager.FindByIdAsync(roleId);
			if(role == null)
			{
				return NotFound(new { Message = $"Role with id={roleId} cannot be found " });
			}
			for(int i = 0; i < model.Count; i++)
			{
				var user = await _userManager.FindByIdAsync(model[i].UserId);
				IdentityResult result;
				if (model[i].IsSelected && !(await _userManager.IsInRoleAsync(user,role.Name)))
				{
					result=await _userManager.AddToRoleAsync(user,role.Name);
				}
				else if (!model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
				{
					result=await _userManager.RemoveFromRoleAsync(user,role.Name);
				}
				else
				{
					continue;
				}

				if (result.Succeeded)
				{
					if (i < (model.Count - 1))
					{
						continue;
					}
					else
					{
						return Ok(result);
					}
				}
			}
			return NoContent();
		}

    }
}
