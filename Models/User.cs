using Google.Cloud.Firestore;

namespace HealthSpark.Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty] public string Id { get; set; } = string.Empty;
        [FirestoreProperty] public string FullName { get; set; } = string.Empty;
        [FirestoreProperty] public string Email { get; set; } = string.Empty;
        [FirestoreProperty] public string Role { get; set; } = string.Empty;
        [FirestoreProperty] public string PhoneNumber { get; set; } = string.Empty;
        [FirestoreProperty] public string DateOfBirth { get; set; } = string.Empty;
        [FirestoreProperty] public string Gender { get; set; } = string.Empty;
        [FirestoreProperty] public string ProfileImageUrl { get; set; } = string.Empty;
        [FirestoreProperty] public bool IsActive { get; set; } = true;
        [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}