using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize] //  Require authorization for this controller
	public class TestController : ControllerBase
	{


		[HttpGet("data")]
		public IActionResult GetData()
		{
			return Ok("This data is protected");
		}
		 
	}
}
