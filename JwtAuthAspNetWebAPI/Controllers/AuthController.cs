using JwtAuthAspNetWebAPI.Core.Dtos;
using JwtAuthAspNetWebAPI.Core.OtherObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthAspNetWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        readonly UserManager<IdentityUser> _userManager;
        readonly RoleManager<IdentityRole> _roleManager;
        readonly IConfiguration _configuration;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Adding_Roles")]
        public async Task<IActionResult> SeedRoles()
        {
            bool isOwnerExists = await _roleManager.RoleExistsAsync(StaticUserRoles.OWNER),
             isAdminExists = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN),
             isUserExists = await _roleManager.RoleExistsAsync(StaticUserRoles.USER);

            if (isOwnerExists && isAdminExists && isUserExists)
            {
                return Ok("Roles Seeding is already Done");
            }

            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.OWNER));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.ADMIN));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.USER));

            return Ok("Role Seeding Done Successfully");
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var isExistsUser = await _userManager.FindByNameAsync(registerDto.UserName);

            if (isExistsUser is not null)
            {
                return BadRequest("UserName is already Exists");
            }

            IdentityUser newUser = new()
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var createUserResult = await _userManager.CreateAsync(newUser, registerDto.Password);

            if (!createUserResult.Succeeded)
            {
                string errorMessage = "User Creation Failed Because: ";

                foreach (var error in createUserResult.Errors)
                {
                    errorMessage += $"# {error.Description}";
                }

                return BadRequest(errorMessage);
            }

            await _userManager.AddToRoleAsync(newUser, StaticUserRoles.USER);

            return Ok("User Created Successfully");
        }
    }
}
