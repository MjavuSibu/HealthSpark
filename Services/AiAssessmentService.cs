using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using HealthSpark.Models;

namespace HealthSpark.Services
{
    public class AiAssessmentService
    {
        private readonly AnthropicClient _client;
        private readonly FirebaseService _firebaseService;
        private const string Model = "claude-sonnet-4-6";

        public AiAssessmentService(AnthropicClient client, FirebaseService firebaseService)
        {
            _client = client;
            _firebaseService = firebaseService;
        }

        public async Task<AiAssessment> AssessSymptomAsync(
    Symptom symptom,
    PatientProfile profile,
    User patient)
        {
            var recentVitals = await _firebaseService
                .GetVitalsByPatientAsync(patient.Id);

            var latestVital = recentVitals.FirstOrDefault();

            var promptText = BuildPrompt(symptom, profile, patient, latestVital);

            var messages = new List<Message>
    {
        new Message
        {
            Role = RoleType.User,
            Content = new List<ContentBase>
            {
                new TextContent
                {
                    Text = promptText
                }
            }
        }
    };

            var parameters = new MessageParameters
            {
                Model = Model,
                MaxTokens = 1024,
                Messages = messages,
                System = new List<SystemMessage>
        {
            new SystemMessage(GetSystemPrompt())
        }
            };

            var response = await _client.Messages.GetClaudeMessageAsync(parameters);

            var responseText = response.Content
                .OfType<TextContent>()
                .FirstOrDefault()?.Text ?? string.Empty;

            return ParseAssessment(responseText);
        }

        // ── Prompt Building ────────────────────────────────

        private string BuildPrompt(
            Symptom symptom,
            PatientProfile profile,
            User patient,
            Vital? latestVital)
        {
            var age = CalculateAge(patient.DateOfBirth);

            var conditions = profile.ExistingConditions.Any()
                ? string.Join(", ", profile.ExistingConditions)
                : "None reported";

            var allergies = profile.Allergies.Any()
                ? string.Join(", ", profile.Allergies)
                : "None reported";

            var vitalsSection = latestVital != null
                ? $@"Most recent vitals:
- Blood Pressure: {latestVital.BloodPressureSystolic}/{latestVital.BloodPressureDiastolic} mmHg
- Heart Rate: {latestVital.HeartRate} bpm
- Blood Glucose: {latestVital.BloodGlucose} mmol/L
- Temperature: {latestVital.Temperature} °C
- Logged: {latestVital.LoggedAt:dd MMM yyyy HH:mm}"
                : "No recent vitals on record.";

            return $@"Please assess the following patient symptom.

Patient Information:
- Age: {age}
- Gender: {patient.Gender}
- Blood Type: {profile.BloodType}
- Existing Conditions: {conditions}
- Known Allergies: {allergies}

{vitalsSection}

Symptom Reported:
- Description: {symptom.Description}
- Body Area: {symptom.BodyArea}
- Severity: {symptom.Severity}
- Logged: {symptom.LoggedAt:dd MMM yyyy HH:mm}

Respond in exactly this format:
URGENCY: [low|moderate|high]
EXPLANATION: [your explanation here]
RECOMMENDATION: [your recommendation here]";
        }

        private string GetSystemPrompt()
        {
            return @"You are a clinical decision support assistant for HealthSpark, 
a health monitoring platform. Your role is to assess patient-reported symptoms 
in the context of their health profile and recent vitals.

Important guidelines:
- You are not replacing a doctor. Always recommend professional review where appropriate.
- Be clear, concise and empathetic in your language.
- Consider the patient's existing conditions and recent vitals in your assessment.
- Urgency levels: low (monitor at home), moderate (book appointment soon), high (seek urgent care).
- Always follow the exact response format requested.";
        }

        // ── Response Parsing ───────────────────────────────

        private AiAssessment ParseAssessment(string response)
        {
            var assessment = new AiAssessment
            {
                GeneratedAt = DateTime.UtcNow,
                ModelUsed = Model
            };

            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.StartsWith("URGENCY:"))
                    assessment.UrgencyLevel = line.Replace("URGENCY:", "").Trim().ToLower();

                else if (line.StartsWith("EXPLANATION:"))
                    assessment.Explanation = line.Replace("EXPLANATION:", "").Trim();

                else if (line.StartsWith("RECOMMENDATION:"))
                    assessment.Recommendation = line.Replace("RECOMMENDATION:", "").Trim();
            }

            return assessment;
        }

        // ── Helpers ────────────────────────────────────────

        private int CalculateAge(string dateOfBirth)
        {
            if (!DateTime.TryParse(dateOfBirth, out var dob))
                return 0;

            var today = DateTime.Today;
            var age = today.Year - dob.Year;

            if (dob.Date > today.AddYears(-age))
                age--;

            return age;
        }
    }
}