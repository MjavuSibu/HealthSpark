using Google.Cloud.Firestore;

namespace HealthSpark.Models
{
    [FirestoreData]
    public class DoctorProfile
    {
        [FirestoreProperty] public string UserId { get; set; } = string.Empty;
        [FirestoreProperty] public string Specialisation { get; set; } = string.Empty;
        [FirestoreProperty] public string LicenseNumber { get; set; } = string.Empty;
        [FirestoreProperty] public string Department { get; set; } = string.Empty;
        [FirestoreProperty] public List<string> AssignedPatientIds { get; set; } = new List<string>();
    }
}