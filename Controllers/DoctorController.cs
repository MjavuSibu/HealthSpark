using HealthSpark.Models;
using HealthSpark.Services;
using HealthSpark.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HealthSpark.Controllers
{
    public class DoctorController : Controller
    {
        private readonly FirebaseService _firebaseService;

        public DoctorController(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        private string GetDoctorId()
        {
            return Request.Cookies["AuthToken"] ?? string.Empty;
        }

        // ── Dashboard ──────────────────────────────────────

        public async Task<IActionResult> Dashboard()
        {
            var doctorId = GetDoctorId();

            var doctor = await _firebaseService.GetUserByIdAsync(doctorId);
            if (doctor == null) return RedirectToAction("Login", "Account");

            var doctorProfile = await _firebaseService.GetDoctorProfileAsync(doctorId);

            var appointments = await _firebaseService
                .GetAppointmentsByDoctorAsync(doctorId);

            var todaysAppointments = appointments
                .Where(a => a.Date == DateTime.UtcNow.ToString("dd MMM yyyy"))
                .ToList();

            var alerts = await _firebaseService.GetAlertsByDoctorAsync(doctorId);

            var assignedPatients = new List<User>();

            if (doctorProfile != null)
            {
                foreach (var patientId in doctorProfile.AssignedPatientIds)
                {
                    var patient = await _firebaseService.GetUserByIdAsync(patientId);
                    if (patient != null) assignedPatients.Add(patient);
                }
            }

            var viewModel = new DashboardViewModel
            {
                CurrentUser = doctor,
                DoctorProfile = doctorProfile,
                TodaysAppointments = todaysAppointments,
                AssignedPatients = assignedPatients,
                Alerts = alerts,
                UnreadAlertsCount = alerts.Count
            };

            return View(viewModel);
        }

        // ── Patients ───────────────────────────────────────

        public async Task<IActionResult> Patients()
        {
            var doctorId = GetDoctorId();
            var doctorProfile = await _firebaseService.GetDoctorProfileAsync(doctorId);

            var assignedPatients = new List<User>();

            if (doctorProfile != null)
            {
                foreach (var patientId in doctorProfile.AssignedPatientIds)
                {
                    var patient = await _firebaseService.GetUserByIdAsync(patientId);
                    if (patient != null) assignedPatients.Add(patient);
                }
            }

            var viewModel = new DashboardViewModel
            {
                AssignedPatients = assignedPatients
            };

            return View(viewModel);
        }

        public async Task<IActionResult> PatientDetail(string patientId)
        {
            var doctorId = GetDoctorId();

            var patient = await _firebaseService.GetUserByIdAsync(patientId);
            if (patient == null) return RedirectToAction("Patients");

            var patientProfile = await _firebaseService.GetPatientProfileAsync(patientId);
            var vitals = await _firebaseService.GetVitalsByPatientAsync(patientId);
            var symptoms = await _firebaseService.GetSymptomsByPatientAsync(patientId);
            var notes = await _firebaseService.GetNotesByPatientAsync(patientId);
            var appointments = await _firebaseService.GetAppointmentsByPatientAsync(patientId);

            var viewModel = new DashboardViewModel
            {
                CurrentUser = patient,
                PatientProfile = patientProfile,
                RecentVitals = vitals,
                LatestVital = vitals.FirstOrDefault(),
                RecentSymptoms = symptoms,
                UpcomingAppointments = appointments
            };

            ViewBag.Notes = notes;

            return View(viewModel);
        }

        // ── Clinical Notes ─────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> AddNote(string patientId, string note, bool isAlert)
        {
            var doctorId = GetDoctorId();

            var clinicalNote = new ClinicalNote
            {
                PatientId = patientId,
                DoctorId = doctorId,
                Note = note,
                IsAlert = isAlert,
                CreatedAt = DateTime.UtcNow
            };

            await _firebaseService.SaveClinicalNoteAsync(clinicalNote);

            if (isAlert)
            {
                var alert = new Alert
                {
                    PatientId = patientId,
                    DoctorId = doctorId,
                    Message = $"New alert from your doctor: {note}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _firebaseService.SaveAlertAsync(alert);
            }

            return RedirectToAction("PatientDetail", new { patientId });
        }

        // ── Appointments ───────────────────────────────────

        public async Task<IActionResult> Appointments()
        {
            var doctorId = GetDoctorId();
            var appointments = await _firebaseService.GetAppointmentsByDoctorAsync(doctorId);

            var viewModel = new DashboardViewModel
            {
                UpcomingAppointments = appointments
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAppointmentStatus(
            string appointmentId, string status)
        {
            await _firebaseService.UpdateAppointmentStatusAsync(appointmentId, status);
            return RedirectToAction("Appointments");
        }

        // ── Alerts ─────────────────────────────────────────

        public async Task<IActionResult> Alerts()
        {
            var doctorId = GetDoctorId();
            var alerts = await _firebaseService.GetAlertsByDoctorAsync(doctorId);

            var viewModel = new DashboardViewModel
            {
                Alerts = alerts,
                UnreadAlertsCount = alerts.Count
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ResolveAlert(string alertId)
        {
            await _firebaseService.MarkAlertAsReadAsync(alertId);
            return RedirectToAction("Alerts");
        }

        // ── AI Assessment Override ─────────────────────────

        [HttpPost]
        public async Task<IActionResult> OverrideAssessment(
            string symptomId, string patientId, string doctorNote)
        {
            var symptoms = await _firebaseService.GetSymptomsByPatientAsync(patientId);
            var symptom = symptoms.FirstOrDefault(s => s.Id == symptomId);

            if (symptom?.AiAssessment != null)
            {
                symptom.AiAssessment.DoctorOverride = true;
                symptom.AiAssessment.DoctorNote = doctorNote;

                await _firebaseService.UpdateSymptomAssessmentAsync(
                    symptomId,
                    symptom.AiAssessment
                );
            }

            return RedirectToAction("PatientDetail", new { patientId });
        }
    }
}