using System.ComponentModel.DataAnnotations;
using HealthSpark.Models;

namespace HealthSpark.ViewModels
{
    public class SymptomAssessmentViewModel
    {
        // ── Form Input ─────────────────────────────────────

        [Required(ErrorMessage = "Please describe your symptom")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a body area")]
        public string BodyArea { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a severity level")]
        public string Severity { get; set; } = string.Empty;

        // ── Assessment Result ──────────────────────────────

        public AiAssessment? Assessment { get; set; }
        public bool AssessmentGenerated { get; set; } = false;
        public bool IsProcessing { get; set; } = false;

        // ── Patient Context ────────────────────────────────

        public User? Patient { get; set; }
        public PatientProfile? PatientProfile { get; set; }
        public Vital? LatestVital { get; set; }

        // ── Display Helpers ────────────────────────────────

        public string UrgencyBadgeClass => Assessment?.UrgencyLevel switch
        {
            "low" => "badge-low",
            "moderate" => "badge-moderate",
            "high" => "badge-high",
            _ => "badge-pending"
        };

        public string UrgencyLabel => Assessment?.UrgencyLevel switch
        {
            "low" => "Low Urgency",
            "moderate" => "Moderate Urgency",
            "high" => "High Urgency",
            _ => "Pending"
        };
    }
}