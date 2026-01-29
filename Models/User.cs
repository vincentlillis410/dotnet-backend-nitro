using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthApi.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
     public string? Provider { get; set; }     // "local" | "google"
     public bool EmailVerified { get; set; } = false;
     public string? VerificationToken { get; set; }
     public DateTime? VerificationTokenExpires { get; set; }
         public string? EmailVerificationCode { get; set; }
         

}
