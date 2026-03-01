using Google.Cloud.Firestore;

namespace HealthSpark.Models
{
    [FirestoreData]
    public class Vital
    {
        [FirestoreProperty] public string Id { get; set; } = string.Empty;
        [FirestoreProperty] public string PatientId { get; set; } = string.Empty;
        [FirestoreProperty] public int BloodPressureSystolic { get; set; } = 0;
        [FirestoreProperty] public int BloodPressureDiastolic { get; set; } = 0;
        [FirestoreProperty] public int HeartRate { get; set; } = 0;
        [FirestoreProperty] public double Weight { get; set; } = 0;
        [FirestoreProperty] public double BloodGlucose { get; set; } = 0;
        [FirestoreProperty] public double Temperature { get; set; } = 0;
        [FirestoreProperty] public double SleepHours { get; set; } = 0;
        [FirestoreProperty] public double WaterIntakeLitres { get; set; } = 0;
        [FirestoreProperty] public bool IsFlagged { get; set; } = false;
        [FirestoreProperty] public string FlagNote { get; set; } = string.Empty;
        [FirestoreProperty] public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    }
}