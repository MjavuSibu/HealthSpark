using Google.Cloud.Firestore;

namespace HealthSpark.Models
{
    [FirestoreData]
    public class AiAssessment
    {
        [FirestoreProperty] public string UrgencyLevel { get; set; } = string.Empty;
        [FirestoreProperty] public string Explanation { get; set; } = string.Empty;
        [FirestoreProperty] public string Recommendation { get; set; } = string.Empty;
        [FirestoreProperty] public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        [FirestoreProperty] public string ModelUsed { get; set; } = string.Empty;
        [FirestoreProperty] public bool DoctorOverride { get; set; } = false;
        [FirestoreProperty] public string DoctorNote { get; set; } = string.Empty;
    }

    [FirestoreData]
    public class Symptom
    {
        [FirestoreProperty] public string Id { get; set; } = string.Empty;
        [FirestoreProperty] public string PatientId { get; set; } = string.Empty;
        [FirestoreProperty] public string Description { get; set; } = string.Empty;
        [FirestoreProperty] public string Severity { get; set; } = string.Empty;
        [FirestoreProperty] public string BodyArea { get; set; } = string.Empty;
        [FirestoreProperty] public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
        [FirestoreProperty] public AiAssessment? AiAssessment { get; set; }
    }
}