using BitirmeTezi.Data;
using BitirmeTezi.Entities;
using BitirmeTezi.Interface;
using BitirmeTezi.ModelsDto.LeaderBoard;
using BitirmeTezi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BitirmeTezi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DailyScoreController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IStudentRepository _repository;

        public DailyScoreController(IStudentRepository repository, DataContext context)
        {
            _repository = repository;
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("get-daily-score")]
        public async Task<IActionResult> GetDailyCategoryScoreboard()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var leaderboardList = await _context.UserDailyScores
                                    .Where(d => d.EarnDate >= today && d.EarnDate < tomorrow)
                                    .ToListAsync();

                var userNickNameList = await _context.Students
                                            .ToDictionaryAsync(
                                                s => s.Id,
                                                s => s.Username
                                            );

                var response = new
                {
                    scoreList = leaderboardList.Select(l => new
                    {
                        username = userNickNameList.ContainsKey(l.UserId) ? userNickNameList[l.UserId] : "Bilinmeyen Kullanıcı",
                        skillType = l.SkillType,
                        levelType = l.LevelType,
                        point = l.Points
                    })
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("post-daily-score")]
        public async Task<IActionResult> PostDailyScore([FromBody]DailyScoreRequestDto dto)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("User claim bulunamadı.");
                }

                Guid userId = Guid.Parse(userIdString!);
                var userIdInt = await _repository.GetIdByGuid(userId);




                var request = new UserDailyScore
                {
                    UserId = userIdInt,
                    Points = dto.Points,
                    SkillType = dto.SkillType,
                    LevelType = dto.LevelType,
                    EarnDate = DateTime.UtcNow
                };

                await _context.UserDailyScores.AddAsync(request);
                await _context.SaveChangesAsync();

                return Ok();

            }
            catch (Exception)
            {
                return StatusCode(500, "Skorbord veri gönderilirken bir hata oluştu.");
            }
        }

    }
}
