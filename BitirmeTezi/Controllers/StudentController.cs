using BitirmeTezi.Auth;
using BCrypt.Net;
using BitirmeTezi.Interface;
using BitirmeTezi.ModelsDto.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Reflection.Metadata.Ecma335;
using System;

namespace BitirmeTezi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentRepository _repository;
        private readonly JwtService _jwtService;

        public StudentController(IStudentRepository repository, JwtService jwtService)
        {
            _repository = repository;
            _jwtService = jwtService;
        }

        [HttpGet("active-students")]
        public async Task<IActionResult> GetActiveStudents()
        {
            try
            {
                var result = await _repository.GetStudentsAsync();

                if (result == null)
                {
                    return NotFound("users-not-found");
                }



                return Ok(result);
            }

            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }

        }

        [HttpPost("login-student")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            try
            {
                var user = await _repository.LoginAsync(dto.Email);

                if (user == null)
                {
                    return BadRequest(new { message = "E-posta veya şifre hatalı" });
                }

                if (user.IsActive == false)
                {
                    return StatusCode(403, new { message = "Kullanıcı Pasif Durumda" });
                }

                var cleanHash = user.PasswordHash.Trim();

                var isPasswordValid = BCrypt.Net.BCrypt.EnhancedVerify(
                        dto.Password,
                        cleanHash
                    );

                if (!isPasswordValid)
                {
                    return BadRequest(new { message = "E-posta veya şifre hatalı" });
                }

                var token = _jwtService.GenerateToken(user);

                return Ok(new TokenDto
                {
                    Token = token
                });
            }

            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        [HttpPost("register-student")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            try
            {
                var passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.Password);
                var result = await _repository.RegisterAsync(dto.Name, dto.Surname, dto.Username, dto.Email, passwordHash, dto.NativeLanguage, dto.Gender);


                switch (result)
                {
                    case -1:
                        return Conflict(new { field = "Email",message = "Email already exists" }); 
                    case -2:
                        return Conflict(new { field = "Username", message = "Username already exists" }); 
                }

                return Ok();

            }

            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [Authorize]
        [HttpGet("log-student")]
        public async Task<IActionResult> LogStudent()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid userId = Guid.Parse(userIdString!);

                var result = await _repository.GetLogStudentAsync(userId);

                if (result == null)
                {
                    return NotFound("user-not-found");
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid userId = Guid.Parse(userIdString!);

                var newPasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.NewPassword);
                var oldPasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.OldPassword);
                var result = await _repository.ChangePasswordAsync(userId, oldPasswordHash, newPasswordHash);

                switch (result)
                {
                    case -1:
                        return BadRequest("wrong-old-password");

                    case 0:
                        return NotFound("user-not-found");
                }

                return Ok(result);

            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [Authorize]
        [HttpPost("change-username")]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameDto dto)
        {
            var username = dto.Username;
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid userId = Guid.Parse(userIdString!);

                var result = await _repository.ChangeUsernameAsync(userId, username);


                switch (result)
                {
                    case -1:
                        return Conflict(new { status = "username-taken" });

                    case 0:
                        return NotFound(new { status = "user-not-found" });

                    case 2:
                        return Ok(new { status = "username-no-changed" });
                }

                return Ok(new { status = "success" });

            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [Authorize]
        [HttpPost("delete-user")]
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid userId = Guid.Parse(userIdString!);

                var result = await _repository.DeleteUserAsync(userId);

                switch (result)
                {
                    case 0:
                        return NotFound(new { status = "user-not-found" });
                    case -1:
                        return Conflict(new { status = "user-already-deleted" });
                    case 2:
                        return Ok(new { status = "user-no-deleted" });
                }

                return Ok(new { status = "success" });

            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }
    }
}
