using Microsoft.AspNetCore.Identity;

namespace HttpServer.Auth
{
    /// <summary>
    /// Simplistic user store with hard coded users and password hashes
    /// </summary>
    public class MemStore : IUserStore<User>, IUserPasswordStore<User>
    {
        Dictionary<string, User> users = new Dictionary<string, User>();
        Dictionary<string, string> userHashes = new Dictionary<string, string>();
        public MemStore()
        {
            AddUser("user@a.com", "AQAAAAEAACcQAAAAENl9Q0KXmLUjbk7IjAVB1Ks+LUcHl6oQ7y7VxeUl40Vqxv95XtlZaz5C6o5x2lbsNQ==");
        }

        void AddUser(string username, string passwordHash)
        {
            users.Add(username.ToLower(), new User(username));
            userHashes.Add(username.ToLower(), passwordHash);
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
            return FindByIdAsync(normalizedUserName.ToLower(), cancellationToken);
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
            return userHashes[user.Name];
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class User
    {
        public string Name { get; set; }

        public User(string name)
        {
            this.Name = name;
        }
    }
}
