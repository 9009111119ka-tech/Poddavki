
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace Poddavki.Data
{
    public class UserRepository
    {
        public List<UserProfile> GetAllUsers()
        {
            var users = new List<UserProfile>();

            using (var connection =
                new SQLiteConnection(DatabaseInitializer.ConnectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(
                    "SELECT * FROM Users", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new UserProfile
                        {
                            Login = reader["Login"].ToString(),
                            PasswordHash = reader["PasswordHash"].ToString(),
                            Salt = reader["Salt"].ToString(),
                            Wins = Convert.ToInt32(reader["Wins"]),
                            Losses = Convert.ToInt32(reader["Losses"]),
                            Draws = Convert.ToInt32(reader["Draws"])
                        });
                    }
                }
            }

            return users;
        }

        public UserProfile Authenticate(string login, string password)
        {
            using (var connection =
                new SQLiteConnection(DatabaseInitializer.ConnectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(
                    "SELECT * FROM Users WHERE Login = @login",
                    connection))
                {
                    command.Parameters.AddWithValue("@login", login);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        string hash = reader["PasswordHash"].ToString();
                        string salt = reader["Salt"].ToString();

                        if (!PasswordHasher.Verify(password, hash, salt))
                            return null;

                        return new UserProfile
                        {
                            Login = reader["Login"].ToString(),
                            PasswordHash = hash,
                            Salt = salt,
                            Wins = Convert.ToInt32(reader["Wins"]),
                            Losses = Convert.ToInt32(reader["Losses"]),
                            Draws = Convert.ToInt32(reader["Draws"])
                        };
                    }
                }
            }
        }

        public bool Register(UserProfile user, string password)
        {
            PasswordHasher.CreateHash(
                password,
                out string hash,
                out string salt);

            using (var connection =
                new SQLiteConnection(DatabaseInitializer.ConnectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(
                    @"INSERT OR IGNORE INTO Users
                    (Login, PasswordHash, Salt, Wins, Losses, Draws)
                    VALUES
                    (@login, @hash, @salt, @wins, @losses, @draws)",
                    connection))
                {
                    command.Parameters.AddWithValue("@login", user.Login);
                    command.Parameters.AddWithValue("@hash", hash);
                    command.Parameters.AddWithValue("@salt", salt);
                    command.Parameters.AddWithValue("@wins", user.Wins);
                    command.Parameters.AddWithValue("@losses", user.Losses);
                    command.Parameters.AddWithValue("@draws", user.Draws);

                    return command.ExecuteNonQuery() == 1;
                }
            }
        }

        public bool UpdateUser(UserProfile user)
        {
            using (var connection =
                new SQLiteConnection(DatabaseInitializer.ConnectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(
                    @"UPDATE Users SET
                    Wins = @wins,
                    Losses = @losses,
                    Draws = @draws
                    WHERE Login = @login",
                    connection))
                {
                    command.Parameters.AddWithValue("@login", user.Login);
                    command.Parameters.AddWithValue("@wins", user.Wins);
                    command.Parameters.AddWithValue("@losses", user.Losses);
                    command.Parameters.AddWithValue("@draws", user.Draws);

                    return command.ExecuteNonQuery() == 1;
                }
            }
        }
        public bool DeleteUser(string login)
        {
            using (var connection =
                new SQLiteConnection(DatabaseInitializer.ConnectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(
                    "DELETE FROM Users WHERE Login = @login",
                    connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    return command.ExecuteNonQuery() == 1;
                }
            }
        }
    }
}
