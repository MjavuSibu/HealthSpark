namespace HealthSpark.Models
{
    public class ClinicalNote
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public bool IsAlert { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}