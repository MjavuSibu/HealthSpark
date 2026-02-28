namespace HealthSpark.Models
{
    public class DoctorProfile
    {
        public string UserId { get; set; } = string.Empty;
        public string Specialisation { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public List<string> AssignedPatientIds { get; set; } = new List<string>();
    }
}