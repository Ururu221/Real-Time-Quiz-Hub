using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtGenerator _jwtGen;

        public AuthController(IUserService userService,
                              IJwtGenerator jwtGen)
        {
            _userService = userService;
            _jwtGen = jwtGen;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (await _userService.UserExists(dto.Email))
                    return Conflict(new { message = "User already exists" });

                var user = await _userService.AddUserAsync(dto);

                var token = _jwtGen.CreateToken(user);

                // create user DTO
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    IsAdmin = user.IsAdmin
                };
                // create response DTO
                var result = new AuthResponseDto
                {
                    Token = token,
                    User = userDto
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // check email and password
                var user = await _userService.ValidateCredentials(dto.Email, dto.Password);
                if (user == null)
                    return Unauthorized(new { message = "Invalid email or password" });

                var token = _jwtGen.CreateToken(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    IsAdmin = user.IsAdmin
                };

                return Ok(new AuthResponseDto
                {
                    Token = token,
                    User = userDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
