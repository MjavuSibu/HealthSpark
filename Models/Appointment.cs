namespace HealthSpark.Models
{
    public class Appointment
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "pending"; // pending | confirmed | cancelled | completed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}