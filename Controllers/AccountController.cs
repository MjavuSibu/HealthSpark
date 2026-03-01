using HealthSpark.Models;
using HealthSpark.Services;
using HealthSpark.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HealthSpark.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly FirebaseService _firebaseService;

        public AccountController(AuthService authService, FirebaseService firebaseService)
        {
            _authService = authService;
            _firebaseService = firebaseService;
        }

        // ── Login ──────────────────────────────────────────

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var (idToken, userId) = await _authService.SignInAsync(
                    model.Email, model.Password);

                var role = await _authService.GetUserRoleFromIdAsync(userId);

                Response.Cookies.Append("AuthToken", userId, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = model.RememberMe
                        ? DateTimeOffset.UtcNow.AddDays(30)
                        : DateTimeOffset.UtcNow.AddHours(8)
                });

                return role switch
                {
                    "admin" => RedirectToAction("Dashboard", "Admin"),
                    "doctor" => RedirectToAction("Dashboard", "Doctor"),
                    "patient" => RedirectToAction("Dashboard", "Patient"),
                    _ => RedirectToAction("Login")
                };
            }
            catch
            {
                model.ErrorMessage = "Invalid email or password. Please try again.";
                return View(model);
            }
        }

        // ── Register ───────────────────────────────────────

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Role = model.Role,
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var userId = await _authService.RegisterUserAsync(user, model.Password);

                if (model.Role == "patient")
                {
                    var patientProfile = new PatientProfile
                    {
                        UserId = userId
                    };

                    await _firebaseService.SavePatientProfileAsync(userId, patientProfile);
                }
                else if (model.Role == "doctor")
                {
                    var doctorProfile = new DoctorProfile
                    {
                        UserId = userId
                    };

                    await _firebaseService.SaveDoctorProfileAsync(userId, doctorProfile);
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"Registration failed: {ex.Message}";
                return View(model);
            }
        }

        // ── Logout ─────────────────────────────────────────

        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            return RedirectToAction("Login");
        }
    }
}