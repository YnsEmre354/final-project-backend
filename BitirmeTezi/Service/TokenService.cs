using System.Security.Cryptography;

namespace BitirmeTezi.Service
{
    /// <summary>
    /// Kriptografik güvenli, URL-safe tek kullanımlık token üretir.
    /// JWT kullanılmıyor çünkü tokenlar DB'de saklanır ve kullanıldıktan sonra silinir.
    /// </summary>
    public class TokenService
    {
        /// <summary>
        /// 64 byte rastgele veri → URL-safe Base64 string (512 bit entropi).
        /// Üretilen token şu karakterleri içerir: A-Z, a-z, 0-9, -, _
        /// </summary>
        public string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}
