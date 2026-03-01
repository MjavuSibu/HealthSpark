using Google.Cloud.Firestore;

namespace HealthSpark.Models
{
    [FirestoreData]
    public class Appointment
    {
        [FirestoreProperty] public string Id { get; set; } = string.Empty;
        [FirestoreProperty] public string PatientId { get; set; } = string.Empty;
        [FirestoreProperty] public string DoctorId { get; set; } = string.Empty;
        [FirestoreProperty] public string Date { get; set; } = string.Empty;
        [FirestoreProperty] public string TimeSlot { get; set; } = string.Empty;
        [FirestoreProperty] public string Reason { get; set; } = string.Empty;
        [FirestoreProperty] public string Status { get; set; } = "pending";
        [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}