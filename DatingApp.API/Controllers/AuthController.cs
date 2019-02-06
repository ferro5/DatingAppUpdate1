using DatingApp.API.Data;
using DatingApp.API.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

public class AuthController : ControllerBase
{
    private readonly IAuthRepository _repo;
    private readonly IConfiguration _config;
    public AuthController(IAuthRepository repo, IConfiguration config)
    {
        _config = config;
        _repo = repo;
    }
    #region register
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
    {
        userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

        if (await _repo.UserExists(userForRegisterDto.Username))
            return BadRequest("Username already exists");

        var userToCreate = new DatingApp.API.Models.User
        {
            UserName = userForRegisterDto.Username
        };

        var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

        return StatusCode(201);
    }
    #endregion
    #region login
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
    {
        var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
        if (userForLoginDto == null) return Unauthorized();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
            new Claim(ClaimTypes.Name,userFromRepo.UserName)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1),
            SigningCredentials = creds
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Ok(new
        {
            token = tokenHandler.WriteToken(token)
        });

    }
    #endregion
}