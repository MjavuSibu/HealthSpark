namespace HealthSpark.Models
{
    public class Vital
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public int BloodPressureSystolic { get; set; } = 0;
        public int BloodPressureDiastolic { get; set; } = 0;
        public int HeartRate { get; set; } = 0;
        public double Weight { get; set; } = 0;
        public double BloodGlucose { get; set; } = 0;
        public double Temperature { get; set; } = 0;
        public double SleepHours { get; set; } = 0;
        public double WaterIntakeLitres { get; set; } = 0;
        public bool IsFlagged { get; set; } = false;
        public string FlagNote { get; set; } = string.Empty;
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    }
}