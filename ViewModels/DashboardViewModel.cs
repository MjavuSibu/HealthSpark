using HealthSpark.Models;

namespace HealthSpark.ViewModels
{
    public class DashboardViewModel
    {
        // ── Shared ─────────────────────────────────────────

        public User CurrentUser { get; set; } = new User();
        public List<Alert> Alerts { get; set; } = new List<Alert>();
        public List<Appointment> UpcomingAppointments { get; set; } = new List<Appointment>();

        // ── Patient Dashboard ──────────────────────────────

        public PatientProfile? PatientProfile { get; set; }
        public Vital? LatestVital { get; set; }
        public List<Vital> RecentVitals { get; set; } = new List<Vital>();
        public List<Symptom> RecentSymptoms { get; set; } = new List<Symptom>();
        public ClinicalNote? LatestDoctorNote { get; set; }

        // ── Doctor Dashboard ───────────────────────────────

        public DoctorProfile? DoctorProfile { get; set; }
        public List<User> AssignedPatients { get; set; } = new List<User>();
        public List<Appointment> TodaysAppointments { get; set; } = new List<Appointment>();
        public List<Vital> FlaggedVitals { get; set; } = new List<Vital>();
        public int UnreadAlertsCount { get; set; } = 0;

        // ── Admin Dashboard ────────────────────────────────

        public int TotalDoctors { get; set; } = 0;
        public int TotalPatients { get; set; } = 0;
        public int TotalAppointmentsToday { get; set; } = 0;
        public int TotalFlaggedReadings { get; set; } = 0;
        public List<User> AllDoctors { get; set; } = new List<User>();
        public List<User> AllPatients { get; set; } = new List<User>();
        public List<User> UnassignedPatients { get; set; } = new List<User>();
    }
}