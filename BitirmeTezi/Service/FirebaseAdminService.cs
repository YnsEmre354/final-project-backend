using FirebaseAdmin.Auth;

namespace BitirmeTezi.Service
{
    /// <summary>
    /// Wraps Firebase Admin SDK token verification.
    /// Returns a typed result so controllers don't depend on FirebaseAdmin directly.
    /// </summary>
    public class FirebaseAdminService
    {
        private readonly ILogger<FirebaseAdminService> _logger;

        public FirebaseAdminService(ILogger<FirebaseAdminService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Verifies a Firebase ID token and returns the decoded claims.
        /// Returns null if the token is invalid or expired.
        /// </summary>
        public async Task<FirebaseTokenResult?> VerifyIdTokenAsync(string idToken)
        {
            try
            {
                var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

                var emailVerified = decoded.Claims.TryGetValue("email_verified", out var ev)
                    && ev is bool b && b;

                decoded.Claims.TryGetValue("email", out var emailObj);
                var email = emailObj?.ToString() ?? decoded.Uid;

                return new FirebaseTokenResult
                {
                    Uid = decoded.Uid,
                    Email = email,
                    EmailVerified = emailVerified
                };
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogWarning("Firebase token verification failed: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Firebase token verification");
                return null;
            }
        }
    }

    public class FirebaseTokenResult
    {
        public string Uid { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
    }
}
