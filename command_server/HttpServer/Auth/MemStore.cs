using Microsoft.AspNetCore.Identity;

namespace HttpServer.Auth
{
    /// <summary>
    /// Simplistic user store with hard coded users and password hashes
    /// </summary>
    public class MemStore : IUserStore<User>, IUserPasswordStore<User>
    {
        Dictionary<string, User> users = new Dictionary<string, User>();
        public MemStore()
        {
            AddUser("user@a.com", "AQAAAAEAACcQAAAAEJZzn8Gbi5yk7Y4BAydV2Rt0O6K3axRhPxgk+ycZLXDxo6X9fIWbyHpNsOGOlT0gxA==");
        }

        void AddUser(string username, string passwordHash)
        {
            users.Add(username.ToLower(), new User(username, passwordHash));
        }

        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            users.TryGetValue(userId, out User user);
            return Task.FromResult(user);
        }

        public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return FindByIdAsync(normalizedUserName, cancellationToken);
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return user.Name;
        }

        public async Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return user.Name;
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return "AQAAAAEAACcQAAAAEJZzn8Gbi5yk7Y4BAydV2Rt0O6K3axRhPxgk+ycZLXDxo6X9fIWbyHpNsOGOlT0gxA==";
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class User
    {
        public string Name { get; set; }
        public string PasswordHash { get; set; }

        public User(string name, string passwordHash)
        {
            this.Name = name;
            this.PasswordHash = passwordHash;
        }
    }
}
