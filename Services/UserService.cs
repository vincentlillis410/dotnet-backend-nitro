using AuthApi.Models;
using AuthApi.Settings;
using MongoDB.Driver;

namespace AuthApi.Services;

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _users = database.GetCollection<User>(settings.UsersCollectionName);
    }
    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _users.Find(u => u.Name == username)
                           .FirstOrDefaultAsync();
    }
    public async Task<User?> GetByEmailAsync(string email)
        => await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task CreateAsync(User user)
        => await _users.InsertOneAsync(user);

    public async Task<User?> GetByVerificationTokenAsync(string token)
    {
        return await _users.Find(u => u.VerificationToken == token).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(User user)
    {
        await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
    }
}
