using Google.Cloud.Firestore;

namespace HealthSpark.Models
{
    [FirestoreData]
    public class ClinicalNote
    {
        [FirestoreProperty] public string Id { get; set; } = string.Empty;
        [FirestoreProperty] public string PatientId { get; set; } = string.Empty;
        [FirestoreProperty] public string DoctorId { get; set; } = string.Empty;
        [FirestoreProperty] public string Note { get; set; } = string.Empty;
        [FirestoreProperty] public bool IsAlert { get; set; } = false;
        [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}