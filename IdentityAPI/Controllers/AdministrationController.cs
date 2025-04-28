using IdentityAPI.JWT;
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
		//Add/Remove user[S] for a Role
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

		[HttpGet("ListUsers")]
		public IActionResult ListUsers()
		{
			var users = _userManager.Users;
			return Ok(users);
		}

		[HttpGet("GetUser")]
		public async Task<IActionResult> GetUser(string id)
		{
			var user=await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return BadRequest();
			}
			return Ok(user);
		}

		[HttpPost("CreateUser")]		 
		public async Task<IActionResult> CreateUser([FromBody] CreateUser model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			// Check if Role Exists First ✅
			if (!await _roleManager.RoleExistsAsync(model.Role))
			{
				return BadRequest(new { Message = $"Role '{model.Role}' does not exist." });
			}

			// Create the user
			var user = new ApplicationUser
			{
				UserName = model.Email,
				Email = model.Email,
				City = model.City
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (!result.Succeeded)
			{
				var errors = result.Errors.Select(e => e.Description);
				return BadRequest(new { Errors = errors });
			}

			// Assign the Role
			var roleResult = await _userManager.AddToRoleAsync(user, model.Role);

			if (!roleResult.Succeeded)
			{
				var errors = roleResult.Errors.Select(e => e.Description);
				return BadRequest(new { Errors = errors });
			}
			 

			return Ok(new
			{
				Message = "User created and role assigned successfully!"
				 
			});
		}

		//edit users email,username,city
		[HttpPost("EditUser")]
		public async Task<IActionResult> EditUser(EditUser model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var user = await _userManager.FindByIdAsync(model.Id);
			if (user == null)
			{
				return NotFound(new { Message = "user cannot be found" });
			}

			user.Email= model.Email;
			user.UserName = model.UserName;
			user.City = model.City;

			var result=await _userManager.UpdateAsync(user);
			if (result.Succeeded)
			{
				return Ok(new { Message = "User updated Succesfully" });
			}

			var errors=result.Errors.Select(e => e.Description);
			return BadRequest(new {Errors = errors});
		}

		[HttpDelete("DeleteUser")]
		public async Task<IActionResult> DeleteUser(string UserId)
		{
			var user = await _userManager.FindByIdAsync(UserId);
			if(user== null)
			{
				return NotFound(new { Message = "user cannot be found" });
			}
			var result=await _userManager.DeleteAsync(user);
			if(result.Succeeded)
			{
				return Ok(new { Message = "User Deleted Succesfully" });
			}
			var errors = result.Errors.Select(e => e.Description);
			return BadRequest(new { Errors = errors });
		}

		[HttpDelete("DeleteRole")]
		public async Task<IActionResult> DeleteRole(string RoleId)
		{
			var role = await _roleManager.FindByIdAsync(RoleId);
			if (role == null)
			{
				return NotFound(new { Message = " role cannot not be found" });
			}
			var result = await _roleManager.DeleteAsync(role);
			if (result.Succeeded)
			{
				return Ok(new { Message = "Role Deleted Succesfully" });
			}
			var errors = result.Errors.Select(e => e.Description);
			return BadRequest(new { Errors = errors });
		}


	}
}
