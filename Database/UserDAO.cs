using System;
using System.Data;
using System.Linq;
using ExemploJWT.Models;

namespace ExemploJWT.Database
{
    public class UserDAO
    {
        private readonly DbHelper dbHelper;

        public UserDAO()
        {
        }

        public UserDAO(DbHelper dbHelper)
        {
            this.dbHelper = dbHelper;
        }

        public User Find(string userId)
        {
            var _params = new object[]
            {
                "@userid", userId
            };

            return dbHelper.Read<User>(
                "select * from users where userid = @userid",
                new Func<IDataReader, User>(reader =>
                    new User
                    {
                        AccessKey = reader["accesskey"].AsString(),
                        UserId = reader["userid"].AsString()
                    }
                ),
                _params).FirstOrDefault();
        }
    }
}