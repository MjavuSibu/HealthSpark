using HealthSpark.Models;
using HealthSpark.Services;
using HealthSpark.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HealthSpark.Controllers
{
    public class AdminController : Controller
    {
        private readonly FirebaseService _firebaseService;
        private readonly AuthService _authService;

        public AdminController(FirebaseService firebaseService, AuthService authService)
        {
            _firebaseService = firebaseService;
            _authService = authService;
        }

        // ── Dashboard ──────────────────────────────────────

        public async Task<IActionResult> Dashboard()
        {
            var allUsers = await _firebaseService.GetAllUsersAsync();

            var doctors = allUsers.Where(u => u.Role == "doctor").ToList();
            var patients = allUsers.Where(u => u.Role == "patient").ToList();

            var allAppointments = new List<Appointment>();
            var allAlerts = new List<Alert>();

            foreach (var doctor in doctors)
            {
                var doctorAppointments = await _firebaseService
                    .GetAppointmentsByDoctorAsync(doctor.Id);
                allAppointments.AddRange(doctorAppointments);

                var doctorAlerts = await _firebaseService
                    .GetAlertsByDoctorAsync(doctor.Id);
                allAlerts.AddRange(doctorAlerts);
            }

            var today = DateTime.UtcNow.Date;

            var unassignedPatients = new List<User>();

            foreach (var patient in patients)
            {
                var profile = await _firebaseService.GetPatientProfileAsync(patient.Id);
                if (profile != null && string.IsNullOrEmpty(profile.AssignedDoctorId))
                    unassignedPatients.Add(patient);
            }

            var viewModel = new DashboardViewModel
            {
                TotalDoctors = doctors.Count,
                TotalPatients = patients.Count,
                TotalAppointmentsToday = allAppointments
                    .Count(a => a.Status == "confirmed"),
                TotalFlaggedReadings = allAlerts.Count,
                AllDoctors = doctors,
                AllPatients = patients,
                UnassignedPatients = unassignedPatients,
                UpcomingAppointments = allAppointments
            };

            return View(viewModel);
        }

        // ── Doctors ────────────────────────────────────────

        public async Task<IActionResult> Doctors()
        {
            var allUsers = await _firebaseService.GetAllUsersAsync();
            var doctors = allUsers.Where(u => u.Role == "doctor").ToList();

            var viewModel = new DashboardViewModel
            {
                AllDoctors = doctors
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterDoctor(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Doctors");

            model.Role = "doctor";

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Role = "doctor",
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var userId = await _authService.RegisterUserAsync(user, model.Password);

            var doctorProfile = new DoctorProfile
            {
                UserId = userId
            };

            await _firebaseService.SaveDoctorProfileAsync(userId, doctorProfile);

            return RedirectToAction("Doctors");
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateDoctor(string userId)
        {
            await _authService.DeactivateUserAsync(userId);
            return RedirectToAction("Doctors");
        }

        [HttpPost]
        public async Task<IActionResult> ReactivateDoctor(string userId)
        {
            await _authService.ReactivateUserAsync(userId);
            return RedirectToAction("Doctors");
        }

        // ── Patients ───────────────────────────────────────

        public async Task<IActionResult> Patients()
        {
            var allUsers = await _firebaseService.GetAllUsersAsync();
            var patients = allUsers.Where(u => u.Role == "patient").ToList();

            var viewModel = new DashboardViewModel
            {
                AllPatients = patients
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPatient(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Patients");

            model.Role = "patient";

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Role = "patient",
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var userId = await _authService.RegisterUserAsync(user, model.Password);

            var patientProfile = new PatientProfile
            {
                UserId = userId
            };

            await _firebaseService.SavePatientProfileAsync(userId, patientProfile);

            return RedirectToAction("Patients");
        }

        [HttpPost]
        public async Task<IActionResult> DeactivatePatient(string userId)
        {
            await _authService.DeactivateUserAsync(userId);
            return RedirectToAction("Patients");
        }

        // ── Assignments ────────────────────────────────────

        public async Task<IActionResult> Assignments()
        {
            var allUsers = await _firebaseService.GetAllUsersAsync();
            var doctors = allUsers.Where(u => u.Role == "doctor").ToList();
            var patients = allUsers.Where(u => u.Role == "patient").ToList();

            var unassignedPatients = new List<User>();

            foreach (var patient in patients)
            {
                var profile = await _firebaseService.GetPatientProfileAsync(patient.Id);
                if (profile != null && string.IsNullOrEmpty(profile.AssignedDoctorId))
                    unassignedPatients.Add(patient);
            }

            var viewModel = new DashboardViewModel
            {
                AllDoctors = doctors,
                AllPatients = patients,
                UnassignedPatients = unassignedPatients
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignPatient(string patientId, string doctorId)
        {
            var patientProfile = await _firebaseService.GetPatientProfileAsync(patientId);

            if (patientProfile != null)
            {
                patientProfile.AssignedDoctorId = doctorId;
                await _firebaseService.SavePatientProfileAsync(patientId, patientProfile);
            }

            var doctorProfile = await _firebaseService.GetDoctorProfileAsync(doctorId);

            if (doctorProfile != null)
            {
                if (!doctorProfile.AssignedPatientIds.Contains(patientId))
                    doctorProfile.AssignedPatientIds.Add(patientId);

                await _firebaseService.SaveDoctorProfileAsync(doctorId, doctorProfile);
            }

            return RedirectToAction("Assignments");
        }

        // ── Alerts ─────────────────────────────────────────

        public async Task<IActionResult> Alerts()
        {
            var allUsers = await _firebaseService.GetAllUsersAsync();
            var doctors = allUsers.Where(u => u.Role == "doctor").ToList();

            var allAlerts = new List<Alert>();

            foreach (var doctor in doctors)
            {
                var doctorAlerts = await _firebaseService.GetAlertsByDoctorAsync(doctor.Id);
                allAlerts.AddRange(doctorAlerts);
            }

            var viewModel = new DashboardViewModel
            {
                Alerts = allAlerts
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ResolveAlert(string alertId)
        {
            await _firebaseService.MarkAlertAsReadAsync(alertId);
            return RedirectToAction("Alerts");
        }
    }
}