using BitirmeTezi.ModelsDto.Question.Reading;
using System.Text.Json;

namespace BitirmeTezi.Service
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;


        public GeminiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<ReadingContentDto> GenerateReadingAsync(string level)
        {
            try
            {
                var baseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash:generateContent";
                var url = $"{baseUrl}?key={_apiKey}";

                var topics = new[] {
                    "günlük rutin", "aile yemeği", "hobiler", "alışveriş",
                    "tatil planları", "iş hayatı", "çevre ve doğa", "teknoloji",
                    "sağlık ve spor", "şehir hayatı", "gelenekler ve kültür", "eğitim"
                };

                var levelDescriptions = new Dictionary<string, string>
                {
                    { "A1", "çok basit ve kısa cümleler, temel kelimeler, geniş zaman ve şimdiki zaman, 3-4 cümlelik paragraf" },
                    { "A2", "basit cümleler, günlük kelimeler, temel zamanlar (geçmiş/şimdiki/gelecek), 5-6 cümlelik paragraf" },
                    { "B1", "orta uzunlukta cümleler, bağlaçlar (ancak, fakat, çünkü, bu nedenle), farklı zaman yapıları, 8-10 cümlelik paragraf" },
                    { "B2", "karmaşık cümle yapıları, soyut kavramlar, pasif yapılar, deyimler, 10-12 cümlelik paragraf" },
                    { "C1", "ileri düzey kelime hazinesi, akademik dil, karmaşık bağlaç yapıları, nüanslı anlatım, 12-15 cümlelik paragraf" },
                    { "C2", "çok ileri düzey, edebi ve akademik dil, atasözleri ve deyimler, retorik yapılar, 15+ cümlelik paragraf" }
                };

                var prompt = $@"
                        Sen bir Türkçe dil öğretmenisin. {level} seviyesinde Türkçe öğrenen yabancılar için okuma metni ve sorular hazırlıyorsun.

                        Seviye özellikleri ({level}): {levelDescriptions[level]}

                        Önce {level} seviyesine uygun, günlük hayattan ilginç bir konu seç.
                        Sonra o konu hakkında metin ve sorular yaz.

                        KURALLAR:
                        - Metin tamamen Türkçe olmalı
                        - Dil seviyesine kesinlikle uygun olmalı, ne çok zor ne çok kolay
                        - Sorular metnin içeriğine dayanmalı, tahminle cevaplanamaz olmalı
                        - Her soru için açıklama Türkçe ve öğretici olmalı
                        - CorrectAnswer sadece şık harfi olmalı: A, B, C veya D
                        - Şıklar kısa ve net olmalı
                        - Yalnızca JSON döndür, başka hiçbir şey yazma

                        Şu JSON yapısını kullan:
                        {{
                          ""Title"": ""string"",
                          ""Paragraph"": ""string"",
                          ""Questions"": [
                            {{ 
                              ""Id"": 1, 
                              ""Text"": ""string"", 
                              ""Options"": [""A) seçenek"", ""B) seçenek"", ""C) seçenek"", ""D) seçenek""], 
                              ""CorrectAnswer"": ""A"", 
                              ""Explanation"": ""Doğru cevap A çünkü metinde ... yazıyor."" 
                            }}
                          ]
                        }}

                        {level} seviyesi için 4 soru üret.";

                var requestData = new
                {
                    contents = new[] { new { parts = new[] { new { text = prompt } } } },
                    generationConfig = new { response_mime_type = "application/json" }
                };

                var response = await _httpClient.PostAsJsonAsync(url, requestData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"--- GOOGLE HATA DETAYI ---");
                    Console.WriteLine(errorContent);

                    throw new Exception($"Gemini API Hatası ({response.StatusCode}): {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(jsonResponse);
                var rawText = doc.RootElement.GetProperty("candidates")[0]
                                             .GetProperty("content")
                                             .GetProperty("parts")[0]
                                             .GetProperty("text").GetString();

                if (string.IsNullOrEmpty(rawText)) throw new Exception("Gemini'dan boş metin döndü.");

                Console.WriteLine("----- GEMINI RAW TEXT -----");
                Console.WriteLine(rawText);
                Console.WriteLine("---------------------------");

                rawText = rawText.Replace("```json", "").Replace("```", "").Trim();


                int firstOpenBrace = rawText.IndexOf('{');
                int lastClosedBrace = rawText.LastIndexOf('}');

                if (firstOpenBrace != -1 && lastClosedBrace != -1)
                {
                    rawText = rawText.Substring(firstOpenBrace, (lastClosedBrace - firstOpenBrace) + 1);
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                return JsonSerializer.Deserialize<ReadingContentDto>(rawText, options);
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Bağlantı Hatası: {httpEx.Message}");
                throw;
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON Parse Hatası: {jsonEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Beklenmedik Hata: {ex.Message}");
                return null;
            }
        }
    }
}
