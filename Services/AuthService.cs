using FirebaseAdmin.Auth;
using HealthSpark.Models;
using System.Text;
using System.Text.Json;

namespace HealthSpark.Services
{
    public class AuthService
    {
        private readonly FirebaseAuth _auth;
        private readonly FirebaseService _firebaseService;
        private readonly string _webApiKey;
        private readonly HttpClient _httpClient;

        public AuthService(
            FirebaseAuth auth,
            FirebaseService firebaseService,
            IConfiguration configuration)
        {
            _auth = auth;
            _firebaseService = firebaseService;
            _webApiKey = configuration["Firebase:WebApiKey"] ?? string.Empty;
            _httpClient = new HttpClient();
        }

        // ── Sign In ────────────────────────────────────────

        public async Task<(string IdToken, string UserId)> SignInAsync(
            string email, string password)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_webApiKey}";

            var payload = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception("Invalid email or password.");

            var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("idToken").GetString() ?? string.Empty;
            var userId = doc.RootElement.GetProperty("localId").GetString() ?? string.Empty;

            return (token, userId);
        }

        // ── Register ───────────────────────────────────────

        public async Task<string> RegisterUserAsync(User user, string password)
        {
            var userArgs = new UserRecordArgs
            {
                Email = user.Email,
                Password = password,
                DisplayName = user.FullName,
                Disabled = false
            };

            var createdUser = await _auth.CreateUserAsync(userArgs);

            user.Id = createdUser.Uid;
            await _firebaseService.SaveUserAsync(createdUser.Uid, user);
            await SetUserRoleAsync(createdUser.Uid, user.Role);

            return createdUser.Uid;
        }

        // ── Role Management ────────────────────────────────

        public async Task SetUserRoleAsync(string userId, string role)
        {
            var claims = new Dictionary<string, object>
            {
                { "role", role }
            };

            await _auth.SetCustomUserClaimsAsync(userId, claims);
        }

        public async Task<string> GetUserRoleFromIdAsync(string userId)
        {
            var user = await _firebaseService.GetUserByIdAsync(userId);
            return user?.Role ?? string.Empty;
        }

        // ── Token Verification ─────────────────────────────

        public async Task<string> GetUserIdFromTokenAsync(string idToken)
        {
            var decodedToken = await _auth.VerifyIdTokenAsync(idToken);
            return decodedToken.Uid;
        }

        public async Task<bool> VerifyTokenAsync(string idToken)
        {
            try
            {
                await _auth.VerifyIdTokenAsync(idToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ── Account Management ─────────────────────────────

        public async Task DeactivateUserAsync(string userId)
        {
            var args = new UserRecordArgs
            {
                Uid = userId,
                Disabled = true
            };

            await _auth.UpdateUserAsync(args);
            await _firebaseService.UpdateUserAsync(userId,
                new Dictionary<string, object> { { "IsActive", false } });
        }

        public async Task ReactivateUserAsync(string userId)
        {
            var args = new UserRecordArgs
            {
                Uid = userId,
                Disabled = false
            };

            await _auth.UpdateUserAsync(args);
            await _firebaseService.UpdateUserAsync(userId,
                new Dictionary<string, object> { { "IsActive", true } });
        }

        public async Task DeleteUserAsync(string userId)
        {
            await _auth.DeleteUserAsync(userId);
        }
    }
}