namespace HealthSpark.Models
{
    public class AiAssessment
    {
        public string UrgencyLevel { get; set; } = string.Empty; // low | moderate | high
        public string Explanation { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string ModelUsed { get; set; } = string.Empty;
        public bool DoctorOverride { get; set; } = false;
        public string DoctorNote { get; set; } = string.Empty;
    }

    public class Symptom
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // mild | moderate | severe
        public string BodyArea { get; set; } = string.Empty;
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
        public AiAssessment? AiAssessment { get; set; }
    }
}