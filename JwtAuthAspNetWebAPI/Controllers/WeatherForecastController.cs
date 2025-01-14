using JwtAuthAspNetWebAPI.Core.OtherObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthAspNetWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };


        [HttpGet]
        [Route("Get")]
        public ActionResult Get()
        {
            return Ok(Summaries); 
        }
        
         
        [HttpGet]
        [Route("GetUser")]
        [Authorize(Roles = StaticUserRoles.USER)]
        public ActionResult GetUser()
        {
            return Ok(Summaries); 
        }
        

        [HttpGet]
        [Route("GetAdmin")]
        [Authorize(Roles = StaticUserRoles.ADMIN)]
        public ActionResult GetAdmin()
        {
            return Ok(Summaries); 
        }


        [HttpGet]
        [Route("GetOwner")]
        [Authorize(Roles = StaticUserRoles.OWNER)]
        public ActionResult GetOwner()
        {
            return Ok(Summaries);
        }
    }
}
