using Devzhou.Data.Entity;

namespace Devzhou.Data.Repository
{
    public class UserRepository : RepositoryBase<User>
    {
        public UserRepository(string connectionString, string databaseName) : base(connectionString, databaseName)
        {
        }
    }
}
