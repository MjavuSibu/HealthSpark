using FirebaseAdmin.Auth;
using HealthSpark.Models;

namespace HealthSpark.Services
{
    public class AuthService
    {
        private readonly FirebaseAuth _auth;
        private readonly FirebaseService _firebaseService;

        public AuthService(FirebaseAuth auth, FirebaseService firebaseService)
        {
            _auth = auth;
            _firebaseService = firebaseService;
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

        public async Task<string> GetUserRoleAsync(string idToken)
        {
            var decodedToken = await _auth.VerifyIdTokenAsync(idToken);
            decodedToken.Claims.TryGetValue("role", out var role);
            return role?.ToString() ?? string.Empty;
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

            await _firebaseService.UpdateUserAsync(userId, new Dictionary<string, object>
            {
                { "IsActive", false }
            });
        }

        public async Task ReactivateUserAsync(string userId)
        {
            var args = new UserRecordArgs
            {
                Uid = userId,
                Disabled = false
            };

            await _auth.UpdateUserAsync(args);

            await _firebaseService.UpdateUserAsync(userId, new Dictionary<string, object>
            {
                { "IsActive", true }
            });
        }

        public async Task DeleteUserAsync(string userId)
        {
            await _auth.DeleteUserAsync(userId);
        }
    }
}