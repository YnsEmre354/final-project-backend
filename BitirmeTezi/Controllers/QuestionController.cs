using BitirmeTezi.Data;
using BitirmeTezi.Interface;
using BitirmeTezi.ModelsDto.Question.Speaking;
using BitirmeTezi.ModelsDto.Question.Writing;
using BitirmeTezi.Service;
using FFMpegCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whisper.net;

namespace BitirmeTezi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private GeminiService _geminiService;
        private GroqService _groqService;
        private DataContext _context;


        public QuestionController(/*GeminiService geminiService*/ GroqService groqService, DataContext context)
        {
            _groqService = groqService;
            //_geminiService = geminiService;
            _context = context;
        }

        [HttpGet("reading/generate/{level}")]
        public async Task<IActionResult> GetReading(string level, [FromQuery] int qId)
        {
            try
            {
                var result = await _groqService.GenerateReadingAsync(level);

                if (result == null)
                {
                    return BadRequest("Model boş değer verdi");
                }

                var selectedQuestion = result.Questions.Where(q => q.Id == qId).ToList();

                if (!selectedQuestion.Any())
                {
                    selectedQuestion = result.Questions.Take(1).ToList();
                }

                var response = new
                {
                    title = result.Title,
                    paragraph = result.Paragraph,
                    questions = selectedQuestion
                };


                return Ok(response);
            }

            catch(Exception ex)
            {
                return BadRequest("Soru üretilirken bir hata oluştu: " + ex.Message);
            }
        }

        [HttpGet("listening/generate/{level}")]
        public async Task<IActionResult> GetListening(string level, [FromQuery] int qId)
        {
            try
            {
                var result = await _groqService.GenerateReadingAsync(level);

                if (result == null)
                {
                    return BadRequest("Model boş değer verdi");
                }

                var selectedQuestion = result.Questions.Where(q => q.Id == qId).ToList();

                if (!selectedQuestion.Any())
                {
                    selectedQuestion = result.Questions.Take(1).ToList();
                }

                var response = new
                {
                    title = result.Title,
                    paragraph = result.Paragraph,
                    questions = selectedQuestion
                };


                return Ok(response);
            }

            catch (Exception ex)
            {
                return BadRequest("Soru üretilirken bir hata oluştu: " + ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet("writing/generate/{level}")]
        public async Task<IActionResult> GetWriting(string level, [FromQuery] int qId)
        {
            try
            {
                var result = await _groqService.GenerateWritingTaskAsync(level);

                if (result == null)
                {
                    return BadRequest("Model boş değer verdi");
                }

                return Ok(result);
            }

            catch (Exception ex)
            {
                return BadRequest("Soru üretilirken bir hata oluştu: " + ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("writing/evaluation")]
        public async Task<IActionResult> EvaluationWriting([FromBody] WritingEvaluationRequestDto request)
        {
            try
            {
                var result = await _groqService.EvaluateWritingAsync(request.Level, request.UserText, request.Topic, request.Instructions);

                if (result == null)
                {
                    return BadRequest("Model boş değer verdi");
                }

                var response = new
                {
                    correctedText = result.CorrectedText,
                    feedback = result.Feedback,
                    detectedLevel = result.DetectedLevel,
                    motivationMessage = result.MotivationMessage,
                    score = result.Score
                };

                return Ok(response);
            }

            catch (Exception e)
            {
                return BadRequest("Soru değerlendirilirken hata oluştu: " + e.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet("speaking/generate/{level}")]
        public async Task<IActionResult> GetSpeaking(string level)
        {
            try
            {
                var result = await _groqService.GenerateSpeakingTaskAsync(level);

                if(result == null)
                {
                    return BadRequest("Model boş değer verdi");
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest("Soru oluşturulurken hata oluştu: " + e.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("speaking/evaluation")]
        public async Task<IActionResult> EvaluateSpeaking([FromForm] SpeakingEvaluationRequestDto request)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var _modelPath = Path.Combine(baseDir, "WhisperModels", "ggml-small.bin");

            if (!System.IO.File.Exists(_modelPath))
                return BadRequest($"Yapay zeka modeli şu yolda bulunamadı: {_modelPath}");

            if (request.AudioFile == null || request.AudioFile.Length == 0)
                return BadRequest("Ses dosyası gönderilmedi.");

            var tempMp3Path = Path.GetTempFileName() + ".mp3";
            var tempWavPath = Path.ChangeExtension(tempMp3Path, ".wav");

            try
            {
                using (var stream = new FileStream(tempMp3Path, FileMode.Create))
                {
                    await request.AudioFile.CopyToAsync(stream);
                }

                FFMpegArguments
                    .FromFileInput(tempMp3Path)
                    .OutputToFile(tempWavPath, false, options => options
                        .WithAudioSamplingRate(16000)
                        .WithCustomArgument("-ac 1")
                        .ForceFormat("wav"))          
                    .ProcessSynchronously();          

                string transcriptText = "";
                using var whisperFactory = WhisperFactory.FromPath(_modelPath);
                using var processor = whisperFactory.CreateBuilder().WithLanguage("tr").Build();
                using var fileStream = System.IO.File.OpenRead(tempWavPath); // WAV dosyasını okuyoruz

                await foreach (var result in processor.ProcessAsync(fileStream))
                {
                    transcriptText += result.Text + " ";
                }
                transcriptText = transcriptText.Trim();

                if (string.IsNullOrEmpty(transcriptText))
                {
                    return BadRequest("Ses kaydından herhangi bir konuşma anlaşılamadı.");
                }

                // 5. LLM Değerlendirme Adımı (Groq/Llama veya Gemini)
                var evaluationResult = await _groqService.EvaluateSpeakingAsync(
                    request.Level,
                    transcriptText,
                    request.Topic,
                    request.Instructions
                );

                if (evaluationResult == null)
                    return StatusCode(500, "Yapay zeka değerlendirmesi oluşturulamadı.");

                var response = new
                {
                    correctedText = evaluationResult.CorrectedText,
                    feedback = evaluationResult.Feedback,
                    detectedLevel = evaluationResult.DetectedLevel,
                    motivationMessage = evaluationResult.MotivationMessage,
                    score = evaluationResult.Score
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"İşlem başarısız: {ex.Message}");
            }
            finally
            {
                if (System.IO.File.Exists(tempMp3Path)) System.IO.File.Delete(tempMp3Path);
                if (System.IO.File.Exists(tempWavPath)) System.IO.File.Delete(tempWavPath);
            }
        }
    }
}
