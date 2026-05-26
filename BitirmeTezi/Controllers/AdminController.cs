using BitirmeTezi.Interface;
using BitirmeTezi.ModelsDto.Student;
using Microsoft.AspNetCore.Mvc;

namespace BitirmeTezi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController: ControllerBase
    {
        private readonly IAdminRepository _repository;
        public AdminController(IAdminRepository repository )
        {
            _repository = repository;
        }


        [HttpPost("login-admin")] 
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            try
            {
                var isLogAdmin = await _repository.LoginAdmin(dto);

                if (isLogAdmin == false)
                {
                    return BadRequest(new { message = "E-posta veya şifre hatalı" });
                }

                return Ok(new
                {
                    message = "Giriş başarılı",
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = "Sunucu hatası: " + e.Message });
            }
        }


        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _repository.GetAllUser();
                return Ok(users);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = "Sunucu hatası: " + e.Message });
            }
        }

        [HttpGet("get-users-progress/{userId}")]
        public async Task<IActionResult> GetUsersProgress(string userId)
        {
            try
            {
                var progressList = await _repository.GetStudentProgressDetails(userId);

                return Ok(progressList);

            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = "Sunucu hatası: " + e.Message });
            }
        }

        [HttpPost("soft-delete-user")]
        public async Task<IActionResult> SoftDeleteUser([FromBody]string userId)
        {
            try
            {
                var isDeleted = await _repository.SoftDeleteUser(userId);

                if(isDeleted == false)
                {
                    return BadRequest("Hata");
                }

                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = "Sunucu hatası: " + e.Message });
            }
        }

        [HttpPost("active-user")]
        public async Task<IActionResult> ActiveUser([FromBody] string userId)
        {
            try
            {
                var isActivated = await _repository.ActiveUser(userId);

                if (isActivated == false)
                {
                    return BadRequest("Hata");
                }

                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = "Sunucu hatası: " + e.Message });
            }
        }


    }
}
