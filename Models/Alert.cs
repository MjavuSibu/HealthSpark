using Google.Cloud.Firestore;

namespace HealthSpark.Models
{
    [FirestoreData]
    public class Alert
    {
        [FirestoreProperty] public string Id { get; set; } = string.Empty;
        [FirestoreProperty] public string PatientId { get; set; } = string.Empty;
        [FirestoreProperty] public string DoctorId { get; set; } = string.Empty;
        [FirestoreProperty] public string VitalId { get; set; } = string.Empty;
        [FirestoreProperty] public string Message { get; set; } = string.Empty;
        [FirestoreProperty] public bool IsRead { get; set; } = false;
        [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}