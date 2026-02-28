namespace HealthSpark.Models
{
    public class PatientProfile
    {
        public string UserId { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public double Height { get; set; } = 0;
        public double Weight { get; set; } = 0;
        public List<string> Allergies { get; set; } = new List<string>();
        public List<string> ExistingConditions { get; set; } = new List<string>();
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string AssignedDoctorId { get; set; } = string.Empty;
    }
}