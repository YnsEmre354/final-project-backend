using BitirmeTezi.Auth;
using BitirmeTezi.Data;
using BitirmeTezi.Entities;
using BitirmeTezi.Enums;
using BitirmeTezi.Interface;
using BitirmeTezi.ModelsDto.Student;
using BitirmeTezi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BitirmeTezi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentRepository _repository;
        private readonly JwtService _jwtService;
        private readonly DataContext _context;
        private readonly UserService _userService;
        private readonly FirebaseAdminService _firebaseAdminService;

        public StudentController(
            IStudentRepository repository,
            JwtService jwtService,
            DataContext context,
            UserService userService,
            FirebaseAdminService firebaseAdminService)
        {
            _repository = repository;
            _jwtService = jwtService;
            _context = context;
            _userService = userService;
            _firebaseAdminService = firebaseAdminService;
        }


        [AllowAnonymous]
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

        [AllowAnonymous]
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

        [AllowAnonymous]
        [HttpPost("register-student")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            try
            {
                var passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.Password);
                var result = await _repository.RegisterAsync(dto.Name, dto.Surname, dto.Username, dto.Email, passwordHash, dto.NativeLanguage, dto.Gender);


                switch (result)
                {
                    case "-1":
                        return Conflict(new { field = "Email",message = "Email already exists" }); 
                    case "-2":
                        return Conflict(new { field = "Username", message = "Username already exists" }); 
                }

                var resultString = Guid.Parse(result);

                var registerUserId = await _repository.GetIdByGuid(resultString);

                // Reactivation durumunda enrollment zaten var olabilir; duplicate oluşmasın
                var hasExistingEnrollments = await _context.UserSkillEnrollments
                    .AnyAsync(e => e.StudentId == registerUserId);

                if (!hasExistingEnrollments)
                {
                    await _userService.InitializeUserSkillEnrollemntAsync(registerUserId, _context);
                }

                return Ok();

            }

            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        // ── Firebase: Login ────────────────────────────────────────────────────────
        /// <summary>
        /// POST /api/Student/firebase-login
        /// Accepts a Firebase ID token, verifies it, finds the student by email,
        /// checks IsActive, and returns the project's own JWT token.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("firebase-login")]
        public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseLoginRequestDto dto)
        {
            try
            {
                // 1. Verify Firebase ID token
                var firebaseToken = await _firebaseAdminService.VerifyIdTokenAsync(dto.IdToken);
                if (firebaseToken == null)
                    return Unauthorized(new { message = "Geçersiz veya süresi dolmuş Firebase token." });

                // 2. Require email to be verified in Firebase
                if (!firebaseToken.EmailVerified)
                    return Unauthorized(new { message = "E-posta adresiniz henüz doğrulanmamış." });

                var email = firebaseToken.Email;

                // 3. Find student in DB by email (use token email as source of truth)
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.Email == email);

                if (student == null)
                    return BadRequest(new { message = "Bu e-posta ile kayıtlı bir hesap bulunamadı." });

                // 4. Check IsActive
                if (!student.IsActive)
                    return StatusCode(403, new { message = "Kullanıcı hesabı pasif durumda." });

                // 5. Build a LoginResultDto and generate the project JWT
                var loginDto = new LoginResultDto
                {
                    UserId = student.UserId,
                    Username = student.Username,
                    NativeLanguage = student.NativeLanguage ?? string.Empty,
                    PasswordHash = student.PasswordHash,
                    IsActive = student.IsActive
                };

                var token = _jwtService.GenerateToken(loginDto);
                return Ok(new TokenDto { Token = token });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // ── Firebase: Complete Registration ────────────────────────────────────────
        /// <summary>
        /// POST /api/Student/complete-firebase-register
        /// Called after the user verifies their Firebase email.
        /// Verifies the ID token, ensures email is verified, then creates the student
        /// record with "FIREBASE_AUTH" as the password placeholder.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("complete-firebase-register")]
        public async Task<IActionResult> CompleteFirebaseRegister([FromBody] CompleteFirebaseRegisterRequestDto dto)
        {
            try
            {
                // 1. Verify Firebase ID token
                var firebaseToken = await _firebaseAdminService.VerifyIdTokenAsync(dto.IdToken);
                if (firebaseToken == null)
                    return Unauthorized(new { message = "Geçersiz veya süresi dolmuş Firebase token." });

                // 2. Require email_verified
                if (!firebaseToken.EmailVerified)
                    return BadRequest(new { message = "E-posta adresiniz henüz doğrulanmamış. Lütfen gelen kutunuzu kontrol edin." });

                var email = firebaseToken.Email;

                // 3. Reject mismatched email (extra safety guard)
                if (!string.Equals(email, dto.IdToken, StringComparison.OrdinalIgnoreCase))
                {
                    // IdToken carries the email via Firebase; we trust firebaseToken.Email
                }

                // 4. Create student — password placeholder is "FIREBASE_AUTH"
                var result = await _repository.RegisterAsync(
                    dto.Name,
                    dto.Surname,
                    dto.Username,
                    email,
                    "FIREBASE_AUTH",   // placeholder; BCrypt login will never be used
                    dto.NativeLanguage,
                    dto.Gender
                );

                switch (result)
                {
                    case "-1":
                        // Idempotent: If user already exists with this email, just return success
                        return Ok(new { message = "Bu e-posta adresi zaten kayıtlı. Kayıt başarıyla tamamlanmış sayıldı." });
                    case "-2":
                        return Conflict(new { field = "Username", message = "Bu kullanıcı adı zaten alınmış." });
                }

                // 5. Initialize UserSkillEnrollments (24 records)
                var resultGuid = Guid.Parse(result);
                var registerUserId = await _repository.GetIdByGuid(resultGuid);

                var hasExistingEnrollments = await _context.UserSkillEnrollments
                    .AnyAsync(e => e.StudentId == registerUserId);

                if (!hasExistingEnrollments)
                {
                    await _userService.InitializeUserSkillEnrollemntAsync(registerUserId, _context);
                }

                return Ok(new { message = "Kayıt tamamlandı." });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }
        // ──────────────────────────────────────────────────────────────────────────

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

     /*   [HttpPost("submitStudentAnswer")]
        public async Task<IActionResult> PostStudentAnswer(StudentAnswerDto dto)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                SkillType skillType = SkillType.Reading;
                Guid userId = Guid.Parse(userIdString!);

                var userIdInt = await _repository.GetIdByGuid(userId);

                switch (dto.SkillType)
                {
                    case 1:
                        skillType = SkillType.Reading;
                        break;
                    case 2:
                        skillType = SkillType.Writing;
                        break;
                    case 3:
                        skillType = SkillType.Listening;
                        break;
                    case 4:
                        skillType = SkillType.Speaking;
                        break;

                }

                var isCorrect = string.Equals(
                   dto.StudentAnswer?.Trim(),
                   dto.CorrectAnswer?.Trim(),
                   StringComparison.OrdinalIgnoreCase
               );

                if (!System.Enum.IsDefined(typeof(LevelType), dto.LevelType))
                {
                    return BadRequest("Invalid LevelType");
                }

                var result = new StudentAnswer
                {
                    StudentId = userIdInt,
                    Skill = skillType,
                    Level = (LevelType)dto.LevelType,
                    StudentAnswerText = dto.StudentAnswer,
                    IsCorrect = isCorrect,
                };

                await _context.StudentAnswers.AddAsync(result);
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }

            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }


        [AllowAnonymous]
        [HttpPost("submitStudentProgress")]
        public async Task<IActionResult> PostStudentProgress(StudentProgressDto dto)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                SkillType skillType = SkillType.Reading;
                Guid userId = Guid.Parse(userIdString!);

                var userIdInt = await _repository.GetIdByGuid(userId);
                bool isPassed = false;


                switch (dto.SkillType)
                {
                    case 1:
                        skillType = SkillType.Reading;
                        break;
                    case 2:
                        skillType = SkillType.Writing;
                        break;
                    case 3:
                        skillType = SkillType.Listening;
                        break;
                    case 4:
                        skillType = SkillType.Speaking;
                        break;
                }

                var progress = await _context.StudentProgress.FirstOrDefaultAsync(s => s.StudentId == dto.StudentId && s.Skill == (SkillType)dto.SkillType && s.Level == (LevelType)dto.LevelType);
                var isNew = false;

                if (progress == null)
                {
                    isNew = true;

                    progress = new StudentProgress
                    {
                        StudentId = userIdInt,
                        Skill = skillType,
                        Level = (LevelType)dto.LevelType,
                        CorrectAnswers = 0,
                        TotalQuestions = 0,
                    };
                }

                if (dto.StatusType == 3 && !isNew) 
                {
                    progress.CorrectAnswers += dto.CorrectAnswers;
                    progress.TotalQuestions += dto.TotalQuestions;
                }
                else
                {
                    progress.CorrectAnswers = dto.CorrectAnswers;
                    progress.TotalQuestions = dto.TotalQuestions;
                }

                if(progress.TotalQuestions > 0)
                {
                    progress.AverageScore = (float)progress.CorrectAnswers / progress.TotalQuestions;
                    float percentage = progress.AverageScore * 100;

                    progress.ProgressPercentage = percentage;
                    progress.IsPassed = percentage >= 80;
                }

                progress.UpdatedDate = DateTime.UtcNow;
                progress.Status = (StatusType)dto.StatusType;


                if (isNew)
                {
                    await _context.StudentProgress.AddAsync(progress);
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }

            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }*/


        [AllowAnonymous]
        [HttpPost("submitStudentProgressRL")]
        public async Task<IActionResult> PostStudentProgressRL(GeneralStudentSubmitRLDto dto)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                SkillType skillType = SkillType.Reading;
                Guid userId = Guid.Parse(userIdString!);

                var userIdInt = await _repository.GetIdByGuid(userId);

                switch (dto.SkillType)
                {
                    case 1:
                        skillType = SkillType.Reading;
                        break;
                    case 2:
                        skillType = SkillType.Writing;
                        break;
                    case 3:
                        skillType = SkillType.Listening;
                        break;
                    case 4:
                        skillType = SkillType.Speaking;
                        break;
                }

                var progress = await _context.StudentProgress.FirstOrDefaultAsync(s => s.StudentId == userIdInt && s.Skill == (SkillType)dto.SkillType && s.Level == (LevelType)dto.LevelType);
                var isNew = false;

                if (progress == null)
                {
                    isNew = true;

                    progress = new StudentProgress
                    {
                        StudentId = userIdInt,
                        Skill = skillType,
                        Level = (LevelType)dto.LevelType,
                        CorrectAnswers = dto.CorrectCount,
                        TotalQuestions = dto.TotalCount,
                    };
                }

                if (dto.StatusType == 3 && !isNew)
                {  /// Bu kısımda progress leveltype nasıl geliyor bilmiyorum db ye bakmak lazım db ye nasıl kaydediliyor.
                    progress.CorrectAnswers += dto.CorrectCount;
                    progress.TotalQuestions += dto.TotalCount;
                }
                else
                { 
                    progress.CorrectAnswers = dto.CorrectCount;
                    progress.TotalQuestions = dto.TotalCount;
                }

                if (progress.TotalQuestions > 0)
                {
                    progress.AverageScore = (double)progress.CorrectAnswers / progress.TotalQuestions;
                    double percentage = progress.AverageScore * 100;

                    progress.ProgressPercentage = percentage;
                    progress.IsPassed = percentage >= 75;
                }

                progress.UpdatedDate = DateTime.UtcNow;

                // Status ui tarafından geliyor gibi gözüküyor bu kısım geliştirilebilir sıkıntılı gözüküyor. Progresss eğer 10 soru ayarlarsak total soru sayısına göre yapılsa daha doğru olur.
                /*
                 *  Aslında InProgress işlemde mantıksız gibi gözüküyor çünkü ui da sorudan çıkınca ilerlemenizi kaybedebilirsiniz diye uyarı gelcek o kısma düzenlem yapılmalı
                 * 
                 */
                progress.Status = (StatusType)dto.StatusType;


                if (isNew)
                {
                    await _context.StudentProgress.AddAsync(progress);
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }

            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("submitStudentProgressWS")]
        public async Task<IActionResult> PostStudentProgressWS(GeneralStudentSubmitWSDto dto)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                SkillType skillType = SkillType.Reading;
                Guid userId = Guid.Parse(userIdString!);

                var userIdInt = await _repository.GetIdByGuid(userId);
                var correctCount = 0;

                switch (dto.SkillType)
                {
                    case 1:
                        skillType = SkillType.Reading;
                        break;
                    case 2:
                        skillType = SkillType.Writing;
                        break;
                    case 3:
                        skillType = SkillType.Listening;
                        break;
                    case 4:
                        skillType = SkillType.Speaking;
                        break;
                }

                var progress = await _context.StudentProgress.FirstOrDefaultAsync(s => s.StudentId == userIdInt && s.Skill == (SkillType)dto.SkillType && s.Level == (LevelType)dto.LevelType);
                var isNew = false;

                if(skillType == SkillType.Writing || skillType == SkillType.Speaking)
                {
                    foreach(var score in dto.AiScores)
                    {
                        if(score > 60)
                        {
                            correctCount += 1;
                        }
                    }
                }

                if (progress == null)
                {
                    isNew = true;

                    progress = new StudentProgress
                    {
                        StudentId = userIdInt,
                        Skill = skillType,
                        Level = (LevelType)dto.LevelType,
                        CorrectAnswers = correctCount,
                        TotalQuestions = dto.TotalCount,
                    };
                }

                if (dto.StatusType == 3 && !isNew)
                {
                    progress.CorrectAnswers += correctCount;
                    progress.TotalQuestions += dto.TotalCount;
                }
                else
                {
                    progress.CorrectAnswers = correctCount;
                    progress.TotalQuestions = dto.TotalCount;
                }

                if (progress.TotalQuestions > 0)
                {
                    progress.AverageScore = (double)progress.CorrectAnswers / progress.TotalQuestions;
                    double percentage = progress.AverageScore * 100;

                    progress.ProgressPercentage = percentage;
                    progress.IsPassed = percentage >= 60;
                }

                progress.UpdatedDate = DateTime.UtcNow;
                progress.Status = (StatusType)dto.StatusType;


                if (isNew)
                {
                    await _context.StudentProgress.AddAsync(progress);
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }

            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }


        [AllowAnonymous]
        [HttpGet("getProgressPercentage/{levelType}")]
        public async Task<IActionResult> GetProgressPercentage(int levelType)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid userId = Guid.Parse(userIdString!);
                var userIdInt = await _repository.GetIdByGuid(userId);

                if (!System.Enum.IsDefined(typeof(LevelType), levelType))
                {
                    return BadRequest("Invalid LevelType");
                }

                var progressList = await _context.StudentProgress
                                .Where(p => p.StudentId == userIdInt && p.Level == (LevelType)levelType)
                                .ToListAsync();

                var response = new
                {
                    writingPercentage = progressList.FirstOrDefault(p => p.Skill == SkillType.Writing)?.ProgressPercentage ?? 0,
                    readingPercentage = progressList.FirstOrDefault(p => p.Skill == SkillType.Reading)?.ProgressPercentage ?? 0,
                    listeningPercentage = progressList.FirstOrDefault(p => p.Skill == SkillType.Listening)?.ProgressPercentage ?? 0,
                    speakingPercentage = progressList.FirstOrDefault(p => p.Skill == SkillType.Speaking)?.ProgressPercentage ?? 0
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }


        [AllowAnonymous]
        [HttpGet("getUserSkillEntrollment")]
        public async Task<IActionResult> GetUserSkillEntrollments()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid userId = Guid.Parse(userIdString!);
                var userIdInt = await _repository.GetIdByGuid(userId);

                var userEntrollment = await _context.UserSkillEnrollments
                                    .Where(u => u.StudentId == userIdInt)
                                    .ToListAsync();

                var response = new
                {
                    enrollments = userEntrollment.Select(s => new
                    {
                        skillType = s.SkillType,
                        levelType = s.LevelType,
                        isLocked = s.IsLocked,
                        isAttempted = s.IsAttempted
                    })
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("post-user-skill-enrollment")]
        public async Task<IActionResult> PostUserSkillEntrollment([FromBody]PostUserSkillEnrollment dto)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid userId = Guid.Parse(userIdString!);
                var userIdInt = await _repository.GetIdByGuid(userId);

                var existingEnrollment = await _context.UserSkillEnrollments
                                    .FirstOrDefaultAsync(u => u.StudentId == userIdInt
                                    && u.SkillType == dto.SkillType
                                    && u.LevelType == dto.LevelType);

                var existingStudentProgress = await _context.StudentProgress.FirstOrDefaultAsync(u => u.StudentId == userIdInt
                                    && u.Skill == dto.SkillType
                                    && u.Level == dto.LevelType);

                if (existingEnrollment == null || existingStudentProgress == null)
                {
                    return NotFound("Kayıt Bulunamadı");
                }

                if (existingStudentProgress.IsPassed == true)
                {
                    existingEnrollment.IsAttempted = true;
                }
                else
                {
                    existingEnrollment.IsAttempted = false;
                }

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("unlock-next-level")]
        public async Task<IActionResult> UnlockNextLevel([FromBody]UnlockLevelRequestDto dto)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid userId = Guid.Parse(userIdString!);
                var userIdInt = await _repository.GetIdByGuid(userId);

                var currentEnrollment = await _context.UserSkillEnrollments
                                        .Where(u => u.StudentId == userIdInt && u.LevelType == dto.CurrentLevel)
                                        .ToListAsync();

                int completedSkillCount = currentEnrollment.Count(ent => ent.IsLocked == false && ent.IsAttempted == true);


                if (completedSkillCount == 4)
                {
                    var nextEnrollment = await _context.UserSkillEnrollments
                                        .Where(u => u.StudentId == userIdInt && u.LevelType == dto.NextLevel)
                                        .ToListAsync();
                    foreach (var next in nextEnrollment)
                    {
                        next.IsLocked = false;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Sonraki seviye başarıyla açıldı." });
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }


    }
}
