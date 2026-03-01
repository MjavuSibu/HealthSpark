using Google.Cloud.Firestore;

namespace HealthSpark.Models
{
    [FirestoreData]
    public class PatientProfile
    {
        [FirestoreProperty] public string UserId { get; set; } = string.Empty;
        [FirestoreProperty] public string BloodType { get; set; } = string.Empty;
        [FirestoreProperty] public double Height { get; set; } = 0;
        [FirestoreProperty] public double Weight { get; set; } = 0;
        [FirestoreProperty] public List<string> Allergies { get; set; } = new List<string>();
        [FirestoreProperty] public List<string> ExistingConditions { get; set; } = new List<string>();
        [FirestoreProperty] public string EmergencyContactName { get; set; } = string.Empty;
        [FirestoreProperty] public string EmergencyContactPhone { get; set; } = string.Empty;
        [FirestoreProperty] public string AssignedDoctorId { get; set; } = string.Empty;
    }
}