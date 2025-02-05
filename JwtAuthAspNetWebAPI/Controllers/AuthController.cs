﻿using JwtAuthAspNetWebAPI.Core.Dtos;
using JwtAuthAspNetWebAPI.Core.Entities;
using JwtAuthAspNetWebAPI.Core.OtherObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthAspNetWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly RoleManager<IdentityRole> _roleManager;
        readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
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

            ApplicationUser newUser = new()
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
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


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);

            if (user is null)
            {
                return Unauthorized("Invalid Cedentials");
            }

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!isPasswordCorrect)
            {
                return Unauthorized("Invalid Cedentials");
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("JWTID", Guid.NewGuid().ToString()),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            string token = GenerateNewJsonWebToken(authClaims);

            return Ok(token);
        }
        private string GenerateNewJsonWebToken(List<Claim> claims)
        {
            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var tokenObject = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddHours(1),
                    claims: claims,
                    signingCredentials: new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256));

            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);

            return token;
        }


        [HttpPost]
        [Route("make-admin")]
        public async Task<IActionResult> MakeAdmin([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)
            {
                return BadRequest("Invalid User Name!");
            }

            await _userManager.AddToRoleAsync(user, StaticUserRoles.ADMIN);

            return Ok("User is now an Admin");
        }


        [HttpPost]
        [Route("make-owner")]
        public async Task<IActionResult> MakeOwner([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)
            {
                return BadRequest("Invalid User Name!");
            }

            await _userManager.AddToRoleAsync(user, StaticUserRoles.OWNER);

            return Ok("User is now an Owner");
        }
    }
}
