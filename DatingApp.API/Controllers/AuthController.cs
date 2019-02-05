using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.DTO;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DatingApp.API.Controllers
{
    [Route("api/controller")]
   [ApiController]
    public class AuthController : ControllerBase
    {
        public readonly IAuthRepository _repository;
        public AuthController(IAuthRepository repository)
        {
            _repository = repository;

        }
        [AllowAnonymous]
        [HttpPost("api/auth/register")]
        public async Task<IActionResult>Register(UserForRegisterDto userForRegisterDto)
        {
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
            if (await _repository.UserExists(userForRegisterDto.Username))
                return BadRequest("user already exists");
            var userToCreate = new User
            {
                UserName = userForRegisterDto.Username
            };
            var createdUser = await _repository.Register(userToCreate, userForRegisterDto.Password);
            return StatusCode(201);
        }
     
    }
}
