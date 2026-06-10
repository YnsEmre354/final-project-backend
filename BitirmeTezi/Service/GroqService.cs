using BitirmeTezi.ModelsDto.Question.Listening;
using BitirmeTezi.ModelsDto.Question.Reading;
using BitirmeTezi.ModelsDto.Question.Speaking;
using BitirmeTezi.ModelsDto.Question.Writing;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
namespace BitirmeTezi.Service
{
    public class GroqService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string url = "https://api.groq.com/openai/v1/chat/completions";



        // Tüm fonksiyonlarda kullanılan ortak seviye tanımları
        private static readonly Dictionary<string, (string grammar, string paragraph, string vocab)> LevelProfiles = new()
        {
            { "A1", (
                grammar: "sadece geniş zaman (–r/–er/–ar) ve şimdiki zaman (–iyor); olumsuz ve soru ekleri; temel iyelik ekleri",
                paragraph: "3-4 kısa cümle, her cümle max 8 kelime",
                vocab: "ilk 500 Türkçe kelime arasından seç (ev, su, git, gel, ye, iç, bak, ver gibi)"
            )},
            { "A2", (
                grammar: "geçmiş zaman (–di), gelecek zaman (–ecek), zorunluluk (–meli), yönelme/ayrılma/bulunma hâlleri",
                paragraph: "5-6 cümle, basit bağlaçlar: ve, ama, sonra",
                vocab: "günlük yaşam: market, ulaşım, aile, hobiler, sayılar, saatler"
            )},
            { "B1", (
                grammar: "geniş zaman hikâyesi, şart kipi (–se/–sa), ki bağlacı, çünkü/ama/ancak/bu yüzden/fakat",
                paragraph: "8-10 cümle, fikir geçişleri belirgin",
                vocab: "iş, okul, seyahat, sağlık, çevre alanlarından orta düzey kelimeler"
            )},
            { "B2", (
                grammar: "pasif yapılar (–ılır/–ilir), istek kipi, ilgeç öbekleri, ileri bağlaçlar: oysa, ne var ki, bununla birlikte",
                paragraph: "10-12 cümle, soyut fikirler, karşılaştırma ve argüman",
                vocab: "soyut kavramlar, deyimler, günlük gazete dili"
            )},
            { "C1", (
                grammar: "rivayet birleşik zamanları, ettirgen ve dönüşlü yapılar, zarf-fiil öbekleri, akademik bağlaçlar",
                paragraph: "12-15 cümle, akademik veya analitik tarz",
                vocab: "akademik ve mesleki kelime hazinesi, kalıp ifadeler"
            )},
            { "C2", (
                grammar: "tüm dilbilgisi yapıları; retorik soru, ironik anlatım, edebi sözdizimi",
                paragraph: "15+ cümle, edebi veya fikir yazısı tarzı",
                vocab: "atasözleri, deyimler, edebi ve felsefi terimler"
            )}
        };

        public GroqService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GroqApiKey"];

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("appsettings.json içinde GroqApiKey bulunamadı!");
            }
        }

        // ─── Ortak yardımcı: API çağrısı yap ve content string döndür ───────────
        private async Task<string?> CallGroqAsync(object requestData)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsJsonAsync(url, requestData);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[GROQ HATA] {response.StatusCode}: {errorBody}");
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);

            var rawText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(rawText))
            {
                Console.WriteLine("[GROQ HATA] Boş içerik döndü.");
                return null;
            }

            // JSON fence temizliği (json_object modunda gelmemeli ama garanti)
            rawText = rawText.Replace("```json", "").Replace("```", "").Trim();

            int start = rawText.IndexOf('{');
            int end = rawText.LastIndexOf('}');
            if (start != -1 && end != -1)
                rawText = rawText.Substring(start, end - start + 1);

            return rawText;
        }

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true
        };

        // ════════════════════════════════════════════════════════════════════════
        // 1. READING
        // ════════════════════════════════════════════════════════════════════════
        public async Task<ReadingContentDto> GenerateReadingAsync(string level)
        {
            try
            {
                var p = LevelProfiles[level];

                var prompt = $@"
                    Sen profesyonel bir Türkçe dil eğitimi içerik yazarısın. Görevin {level} seviyesinde Türkçe öğrenen yabancı bir öğrenci için özgün bir okuma metni ve buna bağlı çoktan seçmeli sorular üretmek.

                    ═══ SEVİYE PROFİLİ ({level}) ═══
                    • Dilbilgisi: {p.grammar}
                    • Metin uzunluğu: {p.paragraph}
                    • Kelime hazinesi: {p.vocab}

                    ═══ METİN KURALLARI ═══
                    • Konu: Günlük hayattan somut ve ilgi çekici bir senaryo seç (örn: bir karakter bir şey yapıyor, bir yere gidiyor, bir sorunla karşılaşıyor). Soyut veya ansiklopedik metin yazma.
                    • Metin {level} seviyesinin dilbilgisi sınırlarını kesinlikle aşmamalı.
                    • Metinde 2-3 adet öğrencinin bilmeyebileceği kelime geçmeli; bunlar bağlamdan çıkarılabilir olmalı.
                    • Metin akıcı ve doğal Türkçe olmalı, yapay veya robot dili kullanma.

                    ═══ SORU KURALLARI ═══
                    Tam olarak 4 soru üret. Her soru için bu ilkeleri uygula:

                    1. SORU TİPLERİ DAĞILIMI (4 soruyu bu tiplere göre yaz):
                       - 1 adet: Metinde açıkça yazılı bir detay sorusu (isim, yer, zaman, sayı gibi somut bilgi)
                       - 1 adet: Neden/Nasıl sorusu (metnin nedenini veya nasılını anlama)
                       - 1 adet: Çıkarım sorusu (metinde doğrudan yazılmayan ama bağlamdan anlaşılan)
                       - 1 adet: Ana fikir veya karakter/durum değerlendirme sorusu

                    2. ŞIKLAR İÇİN ZORUNLU KURALLAR:
                       - 4 şık da dilbilgisel olarak doğru ve aynı formatta olmalı.
                       - Yanlış şıklar tamamen saçma veya alakasız olmamalı; makul ama yanlış olmalı.
                       - Doğru cevap soru metninde ipucu verilmeden bulunabilmeli.
                       - Soru cümlesi içinde cevabın kendisi ya da doğrudan ipucu geçmemeli.
                       - Şıklar birbirinden açıkça belli olacak kadar farklı, ama birbirini çağrıştıracak kadar yakın olsun.

                    3. EXPLANATION: Kısa, öğretici ve metnin hangi cümlesine/bölümüne dayanıldığını açıkla. Sadece 'çünkü metinde yazıyor' deme; tam cümleyi veya ifadeyi referans ver.

                    ═══ JSON FORMATI ═══
                    Yalnızca aşağıdaki JSON yapısını döndür, başka hiçbir şey yazma:
                    {{
                      ""Title"": ""string"",
                      ""Paragraph"": ""string"",
                      ""Questions"": [
                        {{
                          ""Id"": 1,
                          ""Text"": ""string"",
                          ""Options"": [""A) seçenek"", ""B) seçenek"", ""C) seçenek"", ""D) seçenek""],
                          ""CorrectAnswer"": ""A"",
                          ""Explanation"": ""string""
                        }}
                      ]
                    }}";

                var requestData = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new { role = "system", content = "Sen yalnızca geçerli JSON formatında Türkçe dil eğitimi içeriği üreten bir uzmansın. Markdown, açıklama veya ek metin kesinlikle yazmazsın." },
                        new { role = "user", content = prompt }
                    },
                    response_format = new { type = "json_object" },
                    temperature = 0.75
                };

                var rawText = await CallGroqAsync(requestData);
                if (rawText == null) return null;

                return JsonSerializer.Deserialize<ReadingContentDto>(rawText, JsonOpts);
            }
            catch (JsonException ex) { Console.WriteLine($"[Reading JSON Hatası] {ex.Message}"); return null; }
            catch (Exception ex) { Console.WriteLine($"[Reading Hatası] {ex.Message}"); return null; }
        }

        // ════════════════════════════════════════════════════════════════════════
        // 2. LISTENING
        // ════════════════════════════════════════════════════════════════════════
        public async Task<ListeningContentDto> GenerateListeningAsync(string level)
        {
            try
            {
                var p = LevelProfiles[level];

                var prompt = $@"
                    Sen profesyonel bir Türkçe dil eğitimi içerik yazarısın. Görevin {level} seviyesinde Türkçe öğrenen yabancı bir öğrenci için sesli dinleme aktivitesi metni ve buna bağlı sorular üretmek.

                    ═══ SEVİYE PROFİLİ ({level}) ═══
                    • Dilbilgisi: {p.grammar}
                    • Metin uzunluğu: {p.paragraph}
                    • Kelime hazinesi: {p.vocab}

                    ═══ DİNLEME METNİ KURALLARI ═══
                    Bu metin bir TTS (text-to-speech) sistemi tarafından seslendirilecek. Bu yüzden:
                    • Metin konuşma diline yakın, doğal ve akıcı olmalı.
                    • Cümleler yazı dilindeki gibi uzun ve iç içe geçmiş olmamalı.
                    • Diyalog formatı tercih edilebilir (iki karakter konuşuyor), ya da birinci tekil şahıs anlatım.
                    • Kısaltma, sembol veya rakamdan kaçın; sayıları yazıyla yaz (örn: '3' yerine 'üç').
                    • Parantez, tire veya virgüllü uzun listeler kullanma.
                    • Metinde mutlaka isim, yer, zaman ve sayı gibi somut ve ölçülebilir detaylar bulun (sorular bunlara dayanacak).
                    • Metin {level} seviyesinin dilbilgisi sınırlarını kesinlikle aşmamalı.

                    ═══ SORU KURALLARI ═══
                    Tam olarak 4 soru üret. Her soru için bu ilkeleri uygula:

                    1. SORU TİPLERİ DAĞILIMI:
                       - 1 adet: Kim/Ne/Nerede/Ne zaman sorusu (metinde geçen somut bir bilgi)
                       - 1 adet: Kaç/Ne kadar sorusu (metinde geçen bir sayı veya miktar)
                       - 1 adet: Neden/Ne için sorusu (karakterin niyeti veya eylemin sebebi)
                       - 1 adet: Ne oldu/Ne yapacak sorusu (olayın sonucu veya planı)

                    2. ŞIKLAR İÇİN ZORUNLU KURALLAR:
                       - 4 şık da dilbilgisel olarak doğru ve aynı formatta olmalı.
                       - Yanlış şıklar tamamen saçma veya alakasız olmamalı; makul ama yanlış olmalı.
                       - Doğru şık soru cümlesinden veya diğer şıklardan çıkarılabilir olmamalı.
                       - Sayı/isim/yer içeren şıklarda yanlış seçenekler metinde geçen gerçek bilgilere yakın ama farklı olmalı (örn: doğru cevap 'üç' ise yanlış şıklar 'iki', 'dört', 'beş' olabilir).

                    3. EXPLANATION: Metnin hangi cümlesine/diyalog satırına dayanıldığını açıkla. Net ve öğretici ol.

                    ═══ JSON FORMATI ═══
                    Yalnızca aşağıdaki JSON yapısını döndür, başka hiçbir şey yazma:
                    {{
                      ""Title"": ""string"",
                      ""Paragraph"": ""string"",
                      ""Questions"": [
                        {{
                          ""Id"": 1,
                          ""Text"": ""string"",
                          ""Options"": [""A) seçenek"", ""B) seçenek"", ""C) seçenek"", ""D) seçenek""],
                          ""CorrectAnswer"": ""A"",
                          ""Explanation"": ""string""
                        }}
                      ]
                    }}";

                var requestData = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new { role = "system", content = "Sen yalnızca geçerli JSON formatında Türkçe dil eğitimi içeriği üreten bir uzmansın. Markdown, açıklama veya ek metin kesinlikle yazmazsın." },
                        new { role = "user", content = prompt }
                    },
                    response_format = new { type = "json_object" },
                    temperature = 0.75
                };

                var rawText = await CallGroqAsync(requestData);
                if (rawText == null) return null;

                return JsonSerializer.Deserialize<ListeningContentDto>(rawText, JsonOpts);
            }
            catch (JsonException ex) { Console.WriteLine($"[Listening JSON Hatası] {ex.Message}"); return null; }
            catch (Exception ex) { Console.WriteLine($"[Listening Hatası] {ex.Message}"); return null; }
        }

        // ════════════════════════════════════════════════════════════════════════
        // 3. WRITING TASK (Görev Üretme)
        // ════════════════════════════════════════════════════════════════════════
        public async Task<WritingTaskDto> GenerateWritingTaskAsync(string level)
        {
            try
            {
                var p = LevelProfiles[level];

                var prompt = $@"
                    Sen profesyonel bir Türkçe dil eğitimi içerik yazarısın. Görevin {level} seviyesindeki Türkçe öğrenen yabancı bir öğrenci için yazma görevi üretmek.

                    ═══ SEVİYE PROFİLİ ({level}) ═══
                    • Kullanılması gereken dilbilgisi: {p.grammar}
                    • Beklenen uzunluk: {p.paragraph}
                    • Kelime hazinesi: {p.vocab}

                    ═══ GÖREV KURALLARI ═══

                    1. TITLE: Kısa ve motive edici (max 6 kelime). Doğrudan konuyu söylesin.

                    2. INSTRUCTIONS: Öğrenciye tam olarak ne yazması gerektiğini açıkla.
                       Şunları mutlaka belirt:
                       a) Somut senaryo: Soyuk yönerge değil, gerçek bir bağlam ver.
                          Kötü örnek: 'Bir yer hakkında yaz.'
                          İyi örnek: 'Geçen hafta gittiğin veya hayal ettiğin bir şehri anlat. Bu şehirde ne gördün, ne yedin, nerede kaldın?'
                       b) Hangi dilbilgisi yapısını kullanması gerektiği (örn: 'Geçmiş zaman (–di) kullan.')
                       c) Minimum uzunluk (örn: 'En az 5 cümle yaz.')
                       d) Yapısal ipucu (örn: 'Giriş–gelişme–sonuç yapısını kullan.' veya 'Önce kendini tanıt, sonra hobilerini anlat.')

                    3. SUGGESTEDVOCABULARY: Tam olarak 6 kelime/ifade.
                       - Basit sözlük kelimesi değil, bağlamsal ve kullanışlı ifadeler ver.
                       - Görevin konusuyla doğrudan ilgili olmalı.
                       - {level} seviyesine uygun: ne çok kolay ne çok zor.
                       - İyi örnek: 'yola çıkmak', 'ilk izlenim', 'vazgeçmemek'
                       - Kötü örnek: 'gitmek', 'ev', 'iyi'

                    ═══ JSON FORMATI ═══
                    Yalnızca aşağıdaki JSON yapısını döndür, başka hiçbir şey yazma:
                    {{
                      ""Title"": ""string"",
                      ""Instructions"": ""string"",
                      ""TargetLevel"": ""{level}"",
                      ""SuggestedVocabulary"": [""ifade1"", ""ifade2"", ""ifade3"", ""ifade4"", ""ifade5"", ""ifade6""]
                    }}";

                var requestData = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new { role = "system", content = "Sen yalnızca geçerli JSON formatında Türkçe dil eğitimi içeriği üreten bir uzmansın. Markdown, açıklama veya ek metin kesinlikle yazmazsın." },
                        new { role = "user", content = prompt }
                    },
                    response_format = new { type = "json_object" },
                    temperature = 0.8
                };

                var rawText = await CallGroqAsync(requestData);
                if (rawText == null) return null;

                return JsonSerializer.Deserialize<WritingTaskDto>(rawText, JsonOpts);
            }
            catch (JsonException ex) { Console.WriteLine($"[WritingTask JSON Hatası] {ex.Message}"); return null; }
            catch (Exception ex) { Console.WriteLine($"[WritingTask Hatası] {ex.Message}"); return null; }
        }

        // ════════════════════════════════════════════════════════════════════════
        // 4. WRITING EVALUATION (Yazı Değerlendirme)
        // ════════════════════════════════════════════════════════════════════════
        public async Task<WritingEvaluationDto> EvaluateWritingAsync(string level, string userText, string topic, string instructions)
        {
            try
            {
                var p = LevelProfiles[level];

                var prompt = $@"
                    Sen deneyimli bir Türkçe dil eğitmenisin. Aşağıdaki öğrenci metnini pedagojik ve objektif bir şekilde değerlendirmeni istiyorum.

                    ═══ GÖREV BİLGİSİ ═══
                    Konu: {topic}
                    Talimatlar: {instructions}
                    Öğrenci Seviyesi: {level}
                    Beklenen Dilbilgisi: {p.grammar}

                    ═══ ÖĞRENCİNİN YAZDIĞI METİN ═══
                    {userText}

                    ═══ DEĞERLENDİRME TALİMATLARI ═══

                    1. CORRECTEDTEXT:
                       - Öğrencinin metnini {level} seviyesini aşmadan düzelt.
                       - Yalnızca dilbilgisi, yazım ve doğallık hatalarını düzelt.
                       - Öğrencinin fikirlerini ve yapısını koru; senin fikirlerini ekleme.
                       - Eğer metin zaten doğruysa olduğu gibi bırak.

                    2. FEEDBACK (Türkçe, 3-5 cümle):
                       Şu sırayla yaz:
                       a) Göreve uyum: Öğrenci konuyu ve talimatları karşıladı mı?
                       b) Dilbilgisi: Hangi yapıları doğru kullandı, hangi yapılarda hata yaptı? Somut örnek ver.
                       c) Kelime hazinesi: Seviyeye uygun mu, tekrar var mı, güçlü ifadeler var mı?
                       Ton: Yapıcı ve teşvik edici ol. Yıkıcı eleştiri yapma.

                    3. SCORE (0-100):
                       Şu ağırlıklarla hesapla:
                       - Göreve uyum (konuya sadakat + talimatları karşılama): %30
                       - Dilbilgisi doğruluğu: %40
                       - Kelime çeşitliliği ve akıcılık: %30
                       Puanı tam sayı olarak ver.

                    4. DETECTEDLEVEL:
                       Metnin gerçek dilbilgisi kalitesine göre A1/A2/B1/B2/C1/C2 olarak belirt.
                       Kayıt seviyesi {level} olsa bile gerçek kaliteyi yansıt.

                    5. MOTIVATIONMESSAGE (1-2 cümle):
                       Öğrenciye özgü, samimi ve motive edici bir mesaj yaz.
                       'Harika iş!' gibi jenerik ifadelerden kaçın; öğrencinin yaptığı spesifik bir şeyi öv.

                    ═══ JSON FORMATI ═══
                    Yalnızca aşağıdaki JSON yapısını döndür, başka hiçbir şey yazma:
                    {{
                      ""CorrectedText"": ""string"",
                      ""Feedback"": ""string"",
                      ""Score"": number,
                      ""DetectedLevel"": ""string"",
                      ""MotivationMessage"": ""string""
                    }}";

                var requestData = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new { role = "system", content = "Sen yalnızca geçerli JSON formatında detaylı ve pedagojik dil eğitimi geri bildirimi veren bir uzmansın. Markdown, açıklama veya ek metin kesinlikle yazmazsın." },
                        new { role = "user", content = prompt }
                    },
                    response_format = new { type = "json_object" },
                    temperature = 0.3
                };

                var rawText = await CallGroqAsync(requestData);
                if (rawText == null) return null;

                return JsonSerializer.Deserialize<WritingEvaluationDto>(rawText, JsonOpts);
            }
            catch (JsonException ex) { Console.WriteLine($"[Evaluation JSON Hatası] {ex.Message}"); return null; }
            catch (Exception ex) { Console.WriteLine($"[Evaluation Hatası] {ex.Message}"); return null; }
        }

        // ════════════════════════════════════════════════════════════════════════
        // 5. SPEAKING TASK
        // ════════════════════════════════════════════════════════════════════════
        public async Task<SpeakingContentDto> GenerateSpeakingTaskAsync(string level)
        {
            try
            {
                var p = LevelProfiles[level];

                var levelContext = level switch
                {
                    "A1" => "tamamen yeni başlayan; sadece temel kelimeler ve çok basit cümleler kurabiliyor",
                    "A2" => "temel günlük ifadeleri biliyor; kısa ve basit cümleler kurabiliyor",
                    "B1" => "günlük konularda kendini ifade edebiliyor; basit bağlaçlar ve farklı zamanlar kullanabiliyor",
                    "B2" => "akıcı iletişim kurabiliyor; soyut konuları tartışabiliyor, fikirlerini savunabiliyor",
                    "C1" => "ileri düzey; karmaşık yapılar, akademik dil ve nüanslı anlatım kullanabiliyor",
                    "C2" => "anadil konuşucusuna yakın yetkinlikte; retorik ve edebi ifadeler kullanabiliyor",
                    _ => "orta düzey"
                };

                var durationByLevel = level switch
                {
                    "A1" => "45-60 saniye",
                    "A2" => "1-1.5 dakika",
                    "B1" => "2-3 dakika",
                    "B2" => "3-4 dakika",
                    "C1" => "4-5 dakika",
                    "C2" => "5+ dakika",
                    _ => "2-3 dakika"
                };

                var prompt = $@"
                    Sen profesyonel bir Türkçe dil eğitimi içerik yazarısın. Görevin {level} ({levelContext}) seviyesindeki Türkçe öğrencisi için konuşma görevi üretmek.

                    ═══ SEVİYE PROFİLİ ({level}) ═══
                    • Kullanılması gereken dilbilgisi: {p.grammar}
                    • Beklenen süre: {durationByLevel}
                    • Kelime hazinesi: {p.vocab}

                    ═══ GÖREV KURALLARI ═══

                    1. TITLE: Kısa ve motive edici (max 6 kelime). Konuşma görevini açıkça yansıtsın.

                    2. INSTRUCTIONS: Öğrenciye tam olarak ne konuşması gerektiğini açıkla.
                       Şunları mutlaka belirt:
                       a) Somut senaryo: Belirsiz konu değil, net bir bağlam ver.
                          Kötü örnek: 'Bir deneyimini anlat.'
                          İyi örnek: 'Çok sevdiğin bir yemeği tarif et: nasıl yapıldığını, ne zaman yediğini ve neden sevdiğini anlat.'
                       b) Hangi dilbilgisi yapısını kullanması gerektiği: '{p.grammar} yapılarını kullan.'
                       c) Beklenen süre: '{durationByLevel} konuş.'
                       d) Minimum cümle/içerik beklentisi: Örn: 'En az 5 farklı cümle kur.' veya 'En az 3 farklı konu başlığına değin.'

                    3. SUGGESTEDVOCABULARY: Tam olarak 6 ifade.
                       - Görevin konusuyla doğrudan ilgili ve {level} seviyesine uygun olmalı.
                       - Basit sözlük kelimesi değil, bağlamsal ve kullanışlı ifadeler ver.
                       - İyi örnek: 'aklımda kalmak', 'fırsatını bulmak', 'alışkanlık edinmek'
                       - Kötü örnek: 'gitmek', 'güzel', 'olmak'

                    4. SPEAKINGTIPS: Tam olarak 4 ipucu. YALNIZCA dilbilgisel ve söylem düzeyinde olmalı.
                       - Kesinlikle beden dili, göz teması, ses tonu, nefes veya fiziksel ipucu YAZMA.
                       - Her ipucu somut ve uygulanabilir olmalı, soyut tavsiye değil.
                       - İyi örnekler:
                         • 'Fikirleri sıralarken önce / sonra / ardından / en son kelimelerini kullan.'
                         • 'Aynı kelimeyi tekrar etmemek için eş anlamlı ifadeler dene: güzel yerine hoş veya harika de.'
                         • 'Neden–çünkü bağlantısını kur: Bunu seviyorum çünkü...'
                         • 'Konuşmanı şu kalıpla aç: Bugün size ... hakkında konuşmak istiyorum.'
                       - Kötü örnekler: 'Doğal konuş.', 'Kendine güven.', 'Dinleyiciyle göz teması kur.'

                    ═══ JSON FORMATI ═══
                    Yalnızca aşağıdaki JSON yapısını döndür, başka hiçbir şey yazma:
                    {{
                      ""Title"": ""string"",
                      ""Instructions"": ""string"",
                      ""TargetLevel"": ""{level}"",
                      ""SuggestedVocabulary"": [""ifade1"", ""ifade2"", ""ifade3"", ""ifade4"", ""ifade5"", ""ifade6""],
                      ""SpeakingTips"": [""ipucu1"", ""ipucu2"", ""ipucu3"", ""ipucu4""]
                    }}";

                var requestData = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new { role = "system", content = "Sen yalnızca geçerli JSON formatında Türkçe dil eğitimi içeriği üreten bir uzmansın. Markdown, açıklama veya ek metin kesinlikle yazmazsın." },
                        new { role = "user", content = prompt }
                    },
                    response_format = new { type = "json_object" },
                    temperature = 0.75
                };

                var rawText = await CallGroqAsync(requestData);
                if (rawText == null) return null;

                return JsonSerializer.Deserialize<SpeakingContentDto>(rawText, JsonOpts);
            }
            catch (JsonException ex) { Console.WriteLine($"[Speaking JSON Hatası] {ex.Message}"); return null; }
            catch (Exception ex) { Console.WriteLine($"[Speaking Hatası] {ex.Message}"); return null; }
        }
        public async Task<SpeakingEvaluationDto> EvaluateSpeakingAsync(string level, string transcriptText, string topic, string instructions)
        {
            try
            {
                var p = LevelProfiles[level];

                var prompt = $@"
                    Sen deneyimli bir Türkçe konuşma (Speaking) eğitmenisin. Aşağıdaki öğrencinin ses kaydından Whisper AI ile yazıya dökülmüş (transcript) konuşma metnini pedagojik ve objektif bir şekilde değerlendirmeni istiyorum.

                    ═══ GÖREV BİLGİSİ ═══
                    Konu (Soru Talimatı): {topic}
                    Ek Talimatlar: {instructions}
                    Öğrenci Seviyesi: {level}
                    Beklenen Dilbilgisi Yapıları: {p.grammar}

                    ═══ ÖĞRENCİNİN KONUŞMA METNİ (TRANSCRIPT) ═══
                    {transcriptText}

                    ═══ DEĞERLENDİRME TALİMATLARI ═══

                    1. CORRECTEDTEXT:
                       - Öğrencinin konuşmasını {level} seviyesini aşmadan, doğal bir konuşma diline uygun şekilde düzelt.
                       - Noktalama işareti eksikliklerini ve bariz konuşma/dilbilgisi hatalarını gider.
                       - Öğrencinin ana fikrini koru, kendi fikirlerini ekleme.
                       - Eğer konuşma akışı zaten doğru ve doğalsa olduğu gibi bırak.

                    2. FEEDBACK (Türkçe, 3-5 cümle):
                       Şu sırayla yaz:
                       a) Göreve uyum: Öğrenci sorulan soruya (konuya) odaklandı mı ve talimatları yerine getirdi mi?
                       b) Konuşma Dilbilgisi: Cümle yapıları konuşma diline uygun mu? Doğru ve hatalı kullandığı yapılara somut örnek ver.
                       c) Kelime Hazinesi ve Akıcılık: Seviyeye uygun kelimeler seçilmiş mi, kelime tekrarları var mı, kendini ifade ederken takılmış mı?
                       Ton: Yapıcı, samimi ve teşvik edici ol.

                    3. SCORE (0-100):
                       Şu ağırlıklarla hesapla:
                       - Göreve uyum (konuya bağlılık + içeriğin zenginliği): %30
                       - Konuşma Dilbilgisi doğruluğu: %40
                       - Kelime çeşitliliği, akıcılık ve doğal ifade yeteneği: %30
                       Puanı tam sayı olarak ver.

                    4. DETECTEDLEVEL:
                       Konuşmanın gerçek dilbilgisi ve akıcılık kalitesine göre A1/A2/B1/B2/C1/C2 olarak belirt.
                       Kayıt seviyesi {level} olsa bile gerçek performansı yansıt.

                    5. MOTIVATIONMESSAGE (1-2 cümle):
                       Öğrencinin konuşma cesaretini artıracak, samimi ve motive edici bir mesaj yaz.
                       Jenerik ifadeler yerine, öğrencinin kurduğu güzel bir cümleyi veya ifadeyi överek spesifik ol.

                    ═══ JSON FORMATI ═══
                    Yalnızca aşağıdaki JSON yapısını döndür, başka hiçbir şey yazma:
                    {{
                      ""CorrectedText"": ""string"",
                      ""Feedback"": ""string"",
                      ""Score"": number,
                      ""DetectedLevel"": ""string"",
                      ""MotivationMessage"": ""string""
                    }}";

                var requestData = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                new { role = "system", content = "Sen yalnızca geçerli JSON formatında detaylı ve pedagojik dil eğitimi geri bildirimi veren bir uzmansın. Markdown, açıklama veya ek metin kesinlikle yazmazsın." },
                new { role = "user", content = prompt }
            },
                    response_format = new { type = "json_object" },
                    temperature = 0.3
                };

                var rawText = await CallGroqAsync(requestData);
                if (rawText == null) return null;

                return JsonSerializer.Deserialize<SpeakingEvaluationDto>(rawText, JsonOpts);
            }
            catch (JsonException ex) { Console.WriteLine($"[Speaking JSON Hatası] {ex.Message}"); return null; }
            catch (Exception ex) { Console.WriteLine($"[Speaking Değerlendirme Hatası] {ex.Message}"); return null; }
        }
    }
}
/*using BitirmeTezi.ModelsDto.Question;
using System.Net.Http.Headers; 
using System.Text.Json;

namespace BitirmeTezi.Service
{
    public class GroqService
    {
        private readonly HttpClient _httpClient;

        public GroqService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ReadingContentDto> GenerateReadingAsync(string level)
        {
            try
            {

                // 1. Groq, API Key'i URL'de "?key=" olarak değil, Header'da bekler:
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

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

                // 2. Groq/OpenAI için Request Şeması
                var requestData = new
                {
                    model = "llama-3.3-70b-versatile", // En güçlü Türkçe model
                    messages = new[]
                    {
                        new { role = "system", content = "Sen Türkçe dil eğitimi veren bir yapay zekasın ve sadece JSON üretirsin." },
                        new { role = "user", content = prompt }
                    },
                    response_format = new { type = "json_object" }, // JSON dönmesini garantiler
                    temperature = 0.7 // Yaratıcılık oranı
                };

                var response = await _httpClient.PostAsJsonAsync(url, requestData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"--- GROQ HATA DETAYI ---");
                    Console.WriteLine(errorContent);

                    throw new Exception($"Groq API Hatası ({response.StatusCode}): {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);

                // 3. Groq'tan dönen sonucu ayrıştırma yolu (choices[0].message.content)
                var rawText = doc.RootElement
                                 .GetProperty("choices")[0]
                                 .GetProperty("message")
                                 .GetProperty("content").GetString();

                if (string.IsNullOrEmpty(rawText)) throw new Exception("Groq'tan boş metin döndü.");

                Console.WriteLine("----- GROQ RAW TEXT -----");
                Console.WriteLine(rawText);
                Console.WriteLine("---------------------------");

                // Senin JSON temizleme mantığın (Groq JSON modunda zaten temiz döner ama garanti olsun)
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


        public async Task<ListeningContentDto> GenerateListeningAsync(string level)
        {
            try
            {

                // 1. Groq, API Key'i URL'de "?key=" olarak değil, Header'da bekler:
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

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
                        - Metin sesli okunmaya uygun, doğal konuşma diline yakın olmalı
                        - Uzun ve karmaşık cümle yapılarından kaçın 
                        - Paragraf yerine ""diyalog"" veya ""anlatım"" formatı tercih edilebilir
                        - Sorular dinleme becerisi ölçmeli: isim, sayı, yer, zaman gibi somut detaylar
                        - Metnin geneline yayılmış detayları sormalı
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

                // 2. Groq/OpenAI için Request Şeması
                var requestData = new
                {
                    model = "llama-3.3-70b-versatile", // En güçlü Türkçe model
                    messages = new[]
                    {
                        new { role = "system", content = "Sen Türkçe dil eğitimi veren bir yapay zekasın ve sadece JSON üretirsin." },
                        new { role = "user", content = prompt }
                    },
                    response_format = new { type = "json_object" }, // JSON dönmesini garantiler
                    temperature = 0.7 // Yaratıcılık oranı
                };

                var response = await _httpClient.PostAsJsonAsync(url, requestData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"--- GROQ HATA DETAYI ---");
                    Console.WriteLine(errorContent);

                    throw new Exception($"Groq API Hatası ({response.StatusCode}): {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);

                // 3. Groq'tan dönen sonucu ayrıştırma yolu (choices[0].message.content)
                var rawText = doc.RootElement
                                 .GetProperty("choices")[0]
                                 .GetProperty("message")
                                 .GetProperty("content").GetString();

                if (string.IsNullOrEmpty(rawText)) throw new Exception("Groq'tan boş metin döndü.");

                Console.WriteLine("----- GROQ RAW TEXT -----");
                Console.WriteLine(rawText);
                Console.WriteLine("---------------------------");

                // Senin JSON temizleme mantığın (Groq JSON modunda zaten temiz döner ama garanti olsun)
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

                return JsonSerializer.Deserialize<ListeningContentDto>(rawText, options);
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


        // 1. Ödev Oluşturma Fonksiyonu
        public async Task<WritingTaskDto> GenerateWritingTaskAsync(string level)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                var prompt = $@"
                Sen bir Türkçe öğretmenisin. {level} seviyesindeki bir öğrenci için yazma ödevi hazırla.
                Ödev öğrencinin seviyesine uygun, günlük hayattan bir konu olmalı.
            
                KURALLAR:
                - Instructions kısmı net ve yönerge verici olmalı (Örn: 'Şu konuyu anlat, şu zamanı kullan').
                - SuggestedVocabulary listesinde en az 5 tane seviyeye uygun anahtar kelime ver.
                - Yalnızca JSON döndür.

                JSON Yapısı:
                {{
                  ""Title"": ""string"",
                  ""Instructions"": ""string"",
                  ""TargetLevel"": ""{level}"",
                  ""SuggestedVocabulary"": [""kelime1"", ""kelime2""]
            }}";

                var requestData = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                new { role = "system", content = "Sadece JSON formatında Türkçe eğitim içeriği üreten bir asistansın." },
                new { role = "user", content = prompt }
            },
                    response_format = new { type = "json_object" },
                    temperature = 0.8
                };

                var response = await _httpClient.PostAsJsonAsync(url, requestData);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);
                var rawText = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                return JsonSerializer.Deserialize<WritingTaskDto>(rawText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Writing Task Hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<WritingEvaluationDto> EvaluateWritingAsync(string level, string userText, string topic, string instructions)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                var prompt = $@"
                    Sen profesyonel bir Türkçe öğretmenisin. 
                    Öğrenciye verilen görev:
                    Konu: ""{topic}""
                    Talimatlar: ""{instructions}""

                    Öğrencinin Yazdığı Metin: ""{userText}""
                    Öğrencinin Seviyesi: {level}

                    DEĞERLENDİRME KRİTERLERİ:
                    1. Göreve Uygunluk: Öğrenci belirlenen konuya ve talimatlara sadık kalmış mı?
                    2. Dil Seviyesi: Seçilen kelimeler ve yapılar {level} seviyesine uygun mu?
                    3. Dilbilgisi ve Yazım: Ekler, zamanlar ve yazım kuralları doğru mu?

                    KURALLAR:
                    - CorrectedText: Metni {level} seviyesini aşmadan en doğru ve doğal haline getir.
                    - Feedback: Hem dilbilgisi hatalarını hem de konuya uyumunu (görevi yerine getirip getirmediğini) Türkçe ve nazikçe açıkla.
                    - Score: 100 üzerinden bir puan ver (Göreve uyum, gramer ve kelime çeşitliliğini baz al).
                    - DetectedLevel: Metnin kalitesine göre seviyesini belirt.
                    - MotivationMessage: Öğrenciyi teşvik et.

                    Yalnızca JSON döndür.

                    JSON Yapısı:
                    {{
                      ""CorrectedText"": ""string"",
                      ""Feedback"": ""string"",
                      ""Score"": number,
                      ""DetectedLevel"": ""string"",
                      ""MotivationMessage"": ""string""
                    }}";

                var requestData = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                new { role = "system", content = "Sen sadece JSON formatında detaylı ve objektif geri bildirim veren bir dil eğitmenisin." },
                new { role = "user", content = prompt }
            },
                    response_format = new { type = "json_object" },
                    temperature = 0.3 // Puanlama ve değerlendirme daha tutarlı (stabil) olsun diye biraz daha düşürdüm
                };

                var response = await _httpClient.PostAsJsonAsync(url, requestData);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);
                var rawText = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                return JsonSerializer.Deserialize<WritingEvaluationDto>(rawText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Evaluation Hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<SpeakingContentDto> GenerateSpeakingTaskAsync(string level)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                var levelContext = level switch
                {
                    "A1" => "tamamen yeni başlayan, sadece temel kelime ve basit cümleler kurabilen",
                    "A2" => "temel günlük ifadeleri bilen, kısa cümleler kurabilen",
                    "B1" => "günlük konularda kendini ifade edebilen, basit bağlaçlar kullanan",
                    "B2" => "akıcı iletişim kurabilen, soyut konuları tartışabilen",
                    "C1" => "ileri düzey, karmaşık yapılar ve akademik dil kullanan",
                    "C2" => "anadil konuşucusuna yakın yetkinlikte",
                    _ => "orta düzey"
                };

                var prompt = $@"
                    Sen bir dil eğitimi içerik uzmanısın. {level} ({levelContext}) seviyesindeki Türkçe öğrencisi için konuşma görevi üret.

                    GÖREV KURALLARI:
                    1. Title: Kısa, motive edici bir başlık (max 6 kelime).
                    2. Instructions: Öğrenciye tam olarak ne yapması gerektiğini söyle.
                       - Konuşma süresini belirt (örn: 2-3 dakika).
                       - Hangi dilbilgisi yapısını kullanması gerektiğini belirt (örn: geçmiş zaman, koşul cümleleri).
                       - Konuyu somutlaştır: soyut değil, net bir senaryo ver (örn: 'Dün gittiğin bir yeri anlat' değil, 'Geçen hafta gittiğin bir kafede yaşadığın deneyimi anlat, ortamı, siparişini ve orada hissetiklerini geçmiş zaman kullanarak açıkla').
                       - Minimum kaç cümle/kelime beklediğini yaz.
                    3. SuggestedVocabulary: Tam olarak 6 kelime/ifade. Her biri bu seviyede öğrenilmesi öncelikli, göreve doğrudan katkı sağlayan kelimeler olsun. Basit sözlük kelimesi değil, bağlamsal ifadeler tercih et (örn: 'gitmek' değil, 'yola çıkmak').
                    4. SpeakingTips: Tam olarak 4 ipucu. Bunlar yalnızca dilbilgisel ve söylem düzeyinde olmalı:
                       - Telaffuz veya beden diline dair ipucu YAZMA.
                       - Örnek iyi ipuçları: 'Bağlaçları kullan: çünkü, ama, ancak, bu yüzden', 'Fikirleri sıralarken önce/sonra/ardından kelimelerini kullan', 'Aynı kelimeyi tekrar etmemek için eş anlamlı ifadeler dene'.

                    JSON Yapısı (yalnızca bu JSON'u döndür, başka hiçbir şey yazma):
                    {{
                      ""Title"": ""string"",
                      ""Instructions"": ""string"",
                      ""TargetLevel"": ""{level}"",
                      ""SuggestedVocabulary"": [""ifade1"", ""ifade2"", ""ifade3"", ""ifade4"", ""ifade5"", ""ifade6""],
                      ""SpeakingTips"": [""ipucu1"", ""ipucu2"", ""ipucu3"", ""ipucu4""]
                    }}";

                var requestData = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                new
                {
                    role = "system",
                    content = "Sen bir dil eğitimi içerik uzmanısın. Yalnızca geçerli JSON formatında yanıt verirsin. Markdown, açıklama veya ek metin kesinlikle yazmazsın."
                },
                new { role = "user", content = prompt }
            },
                    response_format = new { type = "json_object" },
                    temperature = 0.75
                };

                var response = await _httpClient.PostAsJsonAsync(url, requestData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Hatası [{response.StatusCode}]: {errorBody}");
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(jsonResponse);
                var rawText = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(rawText))
                {
                    Console.WriteLine("API boş içerik döndürdü.");
                    return null;
                }

                var result = JsonSerializer.Deserialize<SpeakingContentDto>(
                    rawText,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return result;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Parse Hatası: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Speaking Task Hatası: {ex.Message}");
                return null;
            }
        }
    }
}*/