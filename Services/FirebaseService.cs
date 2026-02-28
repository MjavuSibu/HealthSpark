using Google.Cloud.Firestore;
using HealthSpark.Models;

namespace HealthSpark.Services
{
    public class FirebaseService
    {
        private readonly FirestoreDb _db;

        public FirebaseService(FirestoreDb db)
        {
            _db = db;
        }

        // ── Users ──────────────────────────────────────────

        public async Task<List<User>> GetAllUsersAsync()
        {
            var snapshot = await _db.Collection("users").GetSnapshotAsync();
            return snapshot.Documents
                .Select(d => d.ConvertTo<User>())
                .ToList();
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            var doc = await _db.Collection("users").Document(userId).GetSnapshotAsync();
            return doc.Exists ? doc.ConvertTo<User>() : null;
        }

        public async Task SaveUserAsync(string userId, User user)
        {
            await _db.Collection("users").Document(userId).SetAsync(user);
        }

        public async Task UpdateUserAsync(string userId, Dictionary<string, object> updates)
        {
            await _db.Collection("users").Document(userId).UpdateAsync(updates);
        }

        // ── Patient Profiles ───────────────────────────────

        public async Task<PatientProfile?> GetPatientProfileAsync(string userId)
        {
            var doc = await _db.Collection("patientProfiles").Document(userId).GetSnapshotAsync();
            return doc.Exists ? doc.ConvertTo<PatientProfile>() : null;
        }

        public async Task SavePatientProfileAsync(string userId, PatientProfile profile)
        {
            await _db.Collection("patientProfiles").Document(userId).SetAsync(profile);
        }

        // ── Doctor Profiles ────────────────────────────────

        public async Task<DoctorProfile?> GetDoctorProfileAsync(string userId)
        {
            var doc = await _db.Collection("doctorProfiles").Document(userId).GetSnapshotAsync();
            return doc.Exists ? doc.ConvertTo<DoctorProfile>() : null;
        }

        public async Task SaveDoctorProfileAsync(string userId, DoctorProfile profile)
        {
            await _db.Collection("doctorProfiles").Document(userId).SetAsync(profile);
        }

        // ── Vitals ─────────────────────────────────────────

        public async Task<List<Vital>> GetVitalsByPatientAsync(string patientId)
        {
            var snapshot = await _db.Collection("vitals")
                .WhereEqualTo("PatientId", patientId)
                .OrderByDescending("LoggedAt")
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(d => d.ConvertTo<Vital>())
                .ToList();
        }

        public async Task SaveVitalAsync(Vital vital)
        {
            await _db.Collection("vitals").AddAsync(vital);
        }

        // ── Symptoms ───────────────────────────────────────

        public async Task<List<Symptom>> GetSymptomsByPatientAsync(string patientId)
        {
            var snapshot = await _db.Collection("symptoms")
                .WhereEqualTo("PatientId", patientId)
                .OrderByDescending("LoggedAt")
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(d => d.ConvertTo<Symptom>())
                .ToList();
        }

        public async Task<string> SaveSymptomAsync(Symptom symptom)
        {
            var doc = await _db.Collection("symptoms").AddAsync(symptom);
            return doc.Id;
        }

        public async Task UpdateSymptomAssessmentAsync(string symptomId, AiAssessment assessment)
        {
            await _db.Collection("symptoms").Document(symptomId).UpdateAsync(
                "AiAssessment", assessment
            );
        }

        // ── Appointments ───────────────────────────────────

        public async Task<List<Appointment>> GetAppointmentsByPatientAsync(string patientId)
        {
            var snapshot = await _db.Collection("appointments")
                .WhereEqualTo("PatientId", patientId)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(d => d.ConvertTo<Appointment>())
                .ToList();
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorAsync(string doctorId)
        {
            var snapshot = await _db.Collection("appointments")
                .WhereEqualTo("DoctorId", doctorId)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(d => d.ConvertTo<Appointment>())
                .ToList();
        }

        public async Task SaveAppointmentAsync(Appointment appointment)
        {
            await _db.Collection("appointments").AddAsync(appointment);
        }

        public async Task UpdateAppointmentStatusAsync(string appointmentId, string status)
        {
            await _db.Collection("appointments").Document(appointmentId).UpdateAsync(
                "Status", status
            );
        }

        // ── Clinical Notes ─────────────────────────────────

        public async Task<List<ClinicalNote>> GetNotesByPatientAsync(string patientId)
        {
            var snapshot = await _db.Collection("clinicalNotes")
                .WhereEqualTo("PatientId", patientId)
                .OrderByDescending("CreatedAt")
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(d => d.ConvertTo<ClinicalNote>())
                .ToList();
        }

        public async Task SaveClinicalNoteAsync(ClinicalNote note)
        {
            await _db.Collection("clinicalNotes").AddAsync(note);
        }

        // ── Alerts ─────────────────────────────────────────

        public async Task<List<Alert>> GetAlertsByDoctorAsync(string doctorId)
        {
            var snapshot = await _db.Collection("alerts")
                .WhereEqualTo("DoctorId", doctorId)
                .WhereEqualTo("IsRead", false)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(d => d.ConvertTo<Alert>())
                .ToList();
        }

        public async Task SaveAlertAsync(Alert alert)
        {
            await _db.Collection("alerts").AddAsync(alert);
        }

        public async Task MarkAlertAsReadAsync(string alertId)
        {
            await _db.Collection("alerts").Document(alertId).UpdateAsync(
                "IsRead", true
            );
        }
    }
}