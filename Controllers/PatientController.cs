using HealthSpark.Models;
using HealthSpark.Services;
using HealthSpark.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HealthSpark.Controllers
{
    public class PatientController : Controller
    {
        private readonly FirebaseService _firebaseService;
        private readonly AiAssessmentService _aiAssessmentService;

        public PatientController(
            FirebaseService firebaseService,
            AiAssessmentService aiAssessmentService)
        {
            _firebaseService = firebaseService;
            _aiAssessmentService = aiAssessmentService;
        }

        private string GetPatientId()
        {
            return Request.Cookies["AuthToken"] ?? string.Empty;
        }

        // ── Dashboard ──────────────────────────────────────

        public async Task<IActionResult> Dashboard()
        {
            var patientId = GetPatientId();

            var patient = await _firebaseService.GetUserByIdAsync(patientId);
            if (patient == null) return RedirectToAction("Login", "Account");

            var patientProfile = await _firebaseService.GetPatientProfileAsync(patientId);
            var vitals = await _firebaseService.GetVitalsByPatientAsync(patientId);
            var symptoms = await _firebaseService.GetSymptomsByPatientAsync(patientId);
            var appointments = await _firebaseService.GetAppointmentsByPatientAsync(patientId);
            var notes = await _firebaseService.GetNotesByPatientAsync(patientId);

            var viewModel = new DashboardViewModel
            {
                CurrentUser = patient,
                PatientProfile = patientProfile,
                LatestVital = vitals.FirstOrDefault(),
                RecentVitals = vitals.Take(5).ToList(),
                RecentSymptoms = symptoms.Take(3).ToList(),
                UpcomingAppointments = appointments
                    .Where(a => a.Status == "confirmed" || a.Status == "pending")
                    .ToList(),
                LatestDoctorNote = notes.FirstOrDefault()
            };

            return View(viewModel);
        }

        // ── Vitals ─────────────────────────────────────────

        public async Task<IActionResult> Vitals()
        {
            var patientId = GetPatientId();
            var patient = await _firebaseService.GetUserByIdAsync(patientId);

            ViewData["CurrentUserName"] = patient?.FullName ?? "";

            var vitals = await _firebaseService.GetVitalsByPatientAsync(patientId);

            var viewModel = new DashboardViewModel
            {
                CurrentUser = patient ?? new User(),
                RecentVitals = vitals,
                LatestVital = vitals.FirstOrDefault()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LogVitals(Vital vital)
        {
            var patientId = GetPatientId();

            vital.PatientId = patientId;
            vital.LoggedAt = DateTime.UtcNow;
            vital.IsFlagged = IsVitalFlagged(vital);

            await _firebaseService.SaveVitalAsync(vital);

            if (vital.IsFlagged)
            {
                var patient = await _firebaseService.GetUserByIdAsync(patientId);
                var profile = await _firebaseService.GetPatientProfileAsync(patientId);

                if (profile != null && !string.IsNullOrEmpty(profile.AssignedDoctorId))
                {
                    var alert = new Alert
                    {
                        PatientId = patientId,
                        DoctorId = profile.AssignedDoctorId,
                        VitalId = vital.Id,
                        Message = BuildFlagMessage(vital),
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _firebaseService.SaveAlertAsync(alert);
                }
            }

            return RedirectToAction("Vitals");
        }

        // ── Symptoms ───────────────────────────────────────

        public async Task<IActionResult> Symptoms()
        {
            var patientId = GetPatientId();
            var patient = await _firebaseService.GetUserByIdAsync(patientId);

            ViewData["CurrentUserName"] = patient?.FullName ?? "";

            var symptoms = await _firebaseService.GetSymptomsByPatientAsync(patientId);

            var viewModel = new DashboardViewModel
            {
                CurrentUser = patient ?? new User(),
                RecentSymptoms = symptoms
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LogSymptom(SymptomAssessmentViewModel model)
        {
            var patientId = GetPatientId();

            if (!ModelState.IsValid)
                return RedirectToAction("Symptoms");

            var patient = await _firebaseService.GetUserByIdAsync(patientId);
            var profile = await _firebaseService.GetPatientProfileAsync(patientId);
            var vitals = await _firebaseService.GetVitalsByPatientAsync(patientId);

            if (patient == null || profile == null)
                return RedirectToAction("Symptoms");

            var symptom = new Symptom
            {
                PatientId = patientId,
                Description = model.Description,
                BodyArea = model.BodyArea,
                Severity = model.Severity,
                LoggedAt = DateTime.UtcNow
            };

            var symptomId = await _firebaseService.SaveSymptomAsync(symptom);

            var assessment = await _aiAssessmentService.AssessSymptomAsync(
                symptom,
                profile,
                patient
            );

            await _firebaseService.UpdateSymptomAssessmentAsync(symptomId, assessment);

            if (assessment.UrgencyLevel == "high" &&
                !string.IsNullOrEmpty(profile.AssignedDoctorId))
            {
                var alert = new Alert
                {
                    PatientId = patientId,
                    DoctorId = profile.AssignedDoctorId,
                    Message = $"High urgency symptom reported: {symptom.Description}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _firebaseService.SaveAlertAsync(alert);
            }

            return RedirectToAction("Symptoms");
        }

        // ── Appointments ───────────────────────────────────

        public async Task<IActionResult> Appointments()
        {
            var patientId = GetPatientId();
            var patient = await _firebaseService.GetUserByIdAsync(patientId);

            ViewData["CurrentUserName"] = patient?.FullName ?? "";

            var appointments = await _firebaseService
                .GetAppointmentsByPatientAsync(patientId);

            var viewModel = new DashboardViewModel
            {
                CurrentUser = patient ?? new User(),
                UpcomingAppointments = appointments
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> BookAppointment(Appointment appointment)
        {
            var patientId = GetPatientId();
            var profile = await _firebaseService.GetPatientProfileAsync(patientId);

            appointment.PatientId = patientId;
            appointment.DoctorId = profile?.AssignedDoctorId ?? string.Empty;
            appointment.Status = "pending";
            appointment.CreatedAt = DateTime.UtcNow;

            await _firebaseService.SaveAppointmentAsync(appointment);

            return RedirectToAction("Appointments");
        }

        [HttpPost]
        public async Task<IActionResult> CancelAppointment(string appointmentId)
        {
            await _firebaseService.UpdateAppointmentStatusAsync(appointmentId, "cancelled");
            return RedirectToAction("Appointments");
        }

        // ── Profile ────────────────────────────────────────

        public async Task<IActionResult> Profile()
        {
            var patientId = GetPatientId();
            var patient = await _firebaseService.GetUserByIdAsync(patientId);
            var profile = await _firebaseService.GetPatientProfileAsync(patientId);

            ViewData["CurrentUserName"] = patient?.FullName ?? "";

            var viewModel = new DashboardViewModel
            {
                CurrentUser = patient ?? new User(),
                PatientProfile = profile
            };

            return View(viewModel);
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(
             PatientProfile profile,
             string ExistingConditionsRaw,
             string AllergiesRaw)
        {
            var patientId = GetPatientId();
            profile.UserId = patientId;

            profile.ExistingConditions = ExistingConditionsRaw?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .ToList() ?? new List<string>();

            profile.Allergies = AllergiesRaw?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .ToList() ?? new List<string>();

            await _firebaseService.SavePatientProfileAsync(patientId, profile);

            return RedirectToAction("Profile");
        }

        // ── Doctor Notes ───────────────────────────────────

        public async Task<IActionResult> DoctorNotes()
        {
            var patientId = GetPatientId();
            var patient = await _firebaseService.GetUserByIdAsync(patientId);
            var notes = await _firebaseService.GetNotesByPatientAsync(patientId);
            var profile = await _firebaseService.GetPatientProfileAsync(patientId);

            ViewData["CurrentUserName"] = patient?.FullName ?? "";

            ViewBag.Notes = notes;
            ViewBag.AlertCount = 0;
            ViewBag.PatientName = patient?.FullName ?? "Patient";

            if (profile != null &&
                !string.IsNullOrEmpty(profile.AssignedDoctorId))
            {
                var doctor = await _firebaseService
                    .GetUserByIdAsync(profile.AssignedDoctorId);
                var doctorProfile = await _firebaseService
                    .GetDoctorProfileAsync(profile.AssignedDoctorId);

                ViewBag.DoctorName = doctor?.FullName ?? "Your Doctor";
                ViewBag.DoctorEmail = doctor?.Email ?? "Not available";
                ViewBag.DoctorSpecialisation = doctorProfile?.Specialisation
                                               ?? "General Practice";
            }

            return View();
        }

        // ── Helpers ────────────────────────────────────────

        private bool IsVitalFlagged(Vital vital)
        {
            if (vital.BloodPressureSystolic > 140) return true;
            if (vital.BloodPressureDiastolic > 90) return true;
            if (vital.HeartRate > 100) return true;
            if (vital.HeartRate < 60 && vital.HeartRate > 0) return true;
            if (vital.BloodGlucose > 11) return true;
            if (vital.Temperature > 38) return true;

            return false;
        }

        private string BuildFlagMessage(Vital vital)
        {
            if (vital.BloodPressureSystolic > 140)
                return $"High blood pressure reading: {vital.BloodPressureSystolic}/{vital.BloodPressureDiastolic} mmHg";

            if (vital.HeartRate > 100)
                return $"Elevated heart rate: {vital.HeartRate} bpm";

            if (vital.HeartRate < 60 && vital.HeartRate > 0)
                return $"Low heart rate: {vital.HeartRate} bpm";

            if (vital.BloodGlucose > 11)
                return $"High blood glucose: {vital.BloodGlucose} mmol/L";

            if (vital.Temperature > 38)
                return $"Elevated temperature: {vital.Temperature} °C";

            return "Abnormal vital reading detected";
        }
    }
}