# Yabancılar İçin Yapay Zeka Destekli Mobil Türkçe Öğrenimi Uygulaması - Backend API

## Hızlı Başlangıç

Projeyi hızlıca çalıştırmak için Firebase servis hesabı JSON dosyanızı hazırlayın ve aşağıdaki komutları kullanın. Gerçek şifreler, API anahtarları veya JWT secret değerleri repo içinde veya `appsettings.json` dosyasında tutulmamalıdır!

**CMD için:**
```cmd
cd "C:\path\to\final-project-backend-ediz-push\BitirmeTezi"
set "FIREBASE_SERVICE_ACCOUNT_PATH=C:\firebase-keys\your-service-account.json"
dotnet run
```

**PowerShell için:**
```powershell
cd "C:\path\to\final-project-backend-ediz-push\BitirmeTezi"
$env:FIREBASE_SERVICE_ACCOUNT_PATH="C:\firebase-keys\your-service-account.json"
dotnet run
```

---

## 1. Proje Özeti
Bu proje, yabancıların Türkçe öğrenmesini kolaylaştırmayı hedefleyen yapay zeka destekli bir mobil uygulamanın backend (API) kısmıdır. Mobil uygulama (Flutter) üzerinden gelen istekler bu API üzerinden işlenir, veritabanına kaydedilir, Firebase Auth ile yetkilendirilir ve AI destekli metin/ses işleme süreçleri (Groq API, FFmpeg) yönetilir.

## 2. Kullanılan Teknolojiler
* **ASP.NET Core 8 Web API:** Projenin temel iskeleti ve API endpointlerinin oluşturulması.
* **Entity Framework Core:** Veritabanı işlemleri (Code-First veya Database-First) için ORM.
* **SQL Server / Azure SQL:** İlişkisel veritabanı yönetimi.
* **Firebase Admin SDK:** Mobil tarafta Firebase üzerinden oluşturulan token'ların doğrulanması.
* **JWT Authentication:** Kullanıcıların yetkilendirilmesi ve endpoint güvenliğinin sağlanması.
* **Groq API:** Hızlı yapay zeka dil modeli (LLM) entegrasyonu.
* **MailKit:** E-posta gönderme (ör: bilgilendirme, hesap yönetimi) işlemleri.
* **FFmpeg / Whisper:** Ses dosyalarının işlenmesi için projede entegre edilmiştir. Uygulama başlatılırken `Xabe.FFmpeg` üzerinden otomatik olarak son sürüm FFmpeg indirilir.

## 3. Proje Klasör Yapısı
* `Controllers/`: API endpointlerini barındıran kontrolcüler (örn: `StudentController.cs`).
* `Service/`: İş kurallarının yer aldığı servis katmanı (`FirebaseAdminService.cs`, `GroqService.cs`, `TokenService.cs` vb.).
* `Repository/`: Veritabanı sorgularının soyutlandığı repository sınıfları.
* `Data/`: Entity Framework `DataContext` dosyası.
* `Entities/`: Veritabanı tablolarına karşılık gelen modeller (`Student.cs`, `UserSkillEnrollment.cs` vb.).
* `ModelsDto/`: İstemci ile sunucu arasındaki veri transfer objeleri (`FirebaseLoginRequestDto`, `CompleteFirebaseRegisterRequestDto` vb.).

## 4. Kimlik Doğrulama Mimarisi
Sistem hem Firebase Authentication hem de kendi backend tarafında yönetilen JWT sistemini birlikte kullanır:
* **Firebase Auth:** Mobil uygulama tarafında kayıt, giriş ve şifre sıfırlama işlemleri Firebase üzerinden yönetilir.
* **Email Verification:** Kullanıcıların e-posta doğrulama işlemleri Firebase tarafından halledilir.
* **Firebase ID Token Doğrulama:** Backend, Firebase'den gelen JWT'yi (`Firebase ID Token`) `Firebase Admin SDK` kullanarak doğrular.
* **Backend JWT Üretimi:** Firebase token'ı doğrulandıktan sonra, backend kendi özel `JWT` (JSON Web Token) değerini üretir ve mobil uygulamaya döner. Mobil uygulama sonraki isteklerde bu backend JWT'sini kullanır.

## 5. Firebase Kayıt Akışı
1. Flutter, Firebase üzerinden kullanıcı oluşturur (Email/Şifre ile).
2. Kullanıcı e-postasını Firebase üzerinden doğrular.
3. Flutter, Firebase ID token'ı alır.
4. Backend `POST /api/Student/complete-firebase-register` endpoint'ine Firebase token'ı ve kullanıcı profil bilgileri gönderilir.
5. Backend, Firebase Admin SDK ile token'ı doğrular.
6. Eğer doğrulama başarılıysa SQL tarafında öğrenci kaydı oluşturulur.
7. Kayıt sonrası öğrenciye ait başlangıç `UserSkillEnrollment` kayıtları veritabanında oluşturulur.

## 6. Firebase Login Akışı
1. Flutter, Firebase üzerinden login işlemini gerçekleştirir.
2. Email'in doğrulanmış (verified) olduğu kontrol edilir.
3. Alınan Firebase ID token backend'e gönderilir.
4. Backend, `POST /api/Student/firebase-login` endpoint'inde bu token'ı doğrular.
5. Doğrulama başarılıysa backend kendi JWT token'ını üretip cevap olarak döner.

## 7. Eklenen / Güncellenen Endpointler
* **`POST /api/Student/complete-firebase-register`**: Firebase üzerinden başarıyla kayıt olmuş ve email'i doğrulanmış kullanıcının backend sistemine (veritabanına) kaydını tamamlamak için kullanılır.
* **`POST /api/Student/firebase-login`**: Firebase üzerinden login olmuş kullanıcının backend JWT'sini alabilmesi için kullanılır.
* *(Eski register ve login endpointleri yerine doğrudan Firebase'e entegre olan bu yeni endpointler kullanılmaktadır.)*

## 8. Şifre Sıfırlama Notu
Mobil uygulamada şifre sıfırlama süreçleri tamamen **Firebase Auth** üzerinden yürütülmektedir. Backend tarafındaki eski password reset sistemiyle karıştırılmamalıdır; backend tarafında kullanıcı şifresi tutulmamaktadır.

## 9. Groq API Dayanıklılığı
* Groq API'sinden gelebilecek hatalar, kullanıcı kayıt veya giriş akışını **bozmamalıdır**.
* `Invalid API key`, `401 Unauthorized` veya JSON parse hatalarında, servis güvenli bir şekilde hatayı loglar ve `null` dönerek uygulamanın diğer kısımlarının çalışmaya devam etmesini sağlar.

## 10. Ortam Değişkenleri ve Konfigürasyon
Projeyi çalıştırırken aşağıdaki yapılandırmalara ve ortam değişkenlerine dikkat edilmelidir:
* **`FIREBASE_SERVICE_ACCOUNT_PATH`**: Firebase Admin SDK için gerekli olan JSON dosyasının tam yolu (ortam değişkeni olarak ayarlanmalıdır).
* **Azure SQL Connection String**: `appsettings.json` içinde yer alan `DefaultConnection` (örn: `PUT_AZURE_SQL_PASSWORD_HERE` şeklinde yer değiştirilmeli).
* **Groq API Key**: `PUT_GROQ_API_KEY_HERE`.
* **JWT Ayarları**: `JwtSettings:Key` değeri gizli tutulmalı (örn: `PUT_JWT_SECRET_HERE`).
* **Mail Ayarları**: `appsettings.Development.json` üzerinden ayarlanmalı, gerçek şifreler repoya gönderilmemelidir.

## 11. Local Çalıştırma
Projeyi yerel ortamda çalıştırmak için aşağıdaki yöntemleri kullanabilirsiniz:

**Portlar:** Uygulama `http://0.0.0.0:5260` ve `https://0.0.0.0:7260` portlarında ayağa kalkar.

* Terminali açın, proje dizinine (`BitirmeTezi.csproj` dosyasının olduğu yere) gidin.
* `FIREBASE_SERVICE_ACCOUNT_PATH` ortam değişkenini belirleyin.
* `dotnet run` komutunu çalıştırın.

## 12. Örnek Başlatma Komutları

**CMD için:**
```cmd
cd "C:\path\to\final-project-backend-ediz-push\BitirmeTezi"
set "FIREBASE_SERVICE_ACCOUNT_PATH=C:\firebase-keys\your-service-account.json"
dotnet run
```

**PowerShell için:**
```powershell
cd "C:\path\to\final-project-backend-ediz-push\BitirmeTezi"
$env:FIREBASE_SERVICE_ACCOUNT_PATH="C:\firebase-keys\your-service-account.json"
dotnet run
```

## 13. Bilinen Hatalar ve Çözümler
* **`address already in use`**: Belirtilen port (5260 veya 7260) başka bir uygulama tarafından kullanılıyor. O uygulamayı kapatın veya portları değiştirin.
* **`Firebase service account path bulunamadı`**: `FIREBASE_SERVICE_ACCOUNT_PATH` değişkenini hatalı verdiniz veya ilgili konumda JSON dosyası yok.
* **`Yanlış Azure SQL hostname`**: Connection string'deki server adresini kontrol edin.
* **`Azure SQL firewall hatası`**: Yerel IP adresiniz Azure portalında veritabanı güvenlik duvarı kurallarına eklenmemiş olabilir.
* **`Groq invalid API key`**: Groq API key'inizin süresi dolmuş veya hatalı tanımlanmış olabilir.
* **Emulator'dan backend'e erişim**: Android Emulator üzerinden lokal backend'e istek atıyorsanız, URL olarak `localhost` yerine `10.0.2.2` kullanmalısınız.

## 14. Güvenlik Notları
* Gerçek API key, SQL şifresi, JWT secret, mail şifresi ve Firebase Admin private key bilgileri kesinlikle `README.md`'ye yazılmayacaktır.
* Örneklerde sadece yer tutucular (placeholder) kullanılmıştır.
* `appsettings.json` veya `appsettings.Development.json` içerisinde gerçek gizli değerler tutulmaması şiddetle önerilir.

## 15. Son Değişiklik Özeti
* **Firebase Admin SDK eklendi:** Kimlik doğrulama için backend entegrasyonu tamamlandı.
* **Firebase login/register DTO'ları eklendi:** Veri transferi için modeller oluşturuldu.
* **StudentController güncellendi:** Firebase akışına uygun hale getirildi.
* **Groq hata dayanıklılığı artırıldı:** İstisnaların uygulamanın akışını bozması engellendi.
* **HTTPS redirect problemi çözüldü:** Development ortamında yönlendirme kaynaklı hatalar giderildi.
* **Kayıt akışı idempotent hale getirildi:** Aynı isteğin mükerrer işlemlere sebep olmasının önüne geçildi.

---

## Test Edilen Akışlar
* Firebase kayıt
* Email verification
* Backend complete register
* Azure SQL kayıt
* Firebase login
* Backend JWT alma
* Firebase password reset
