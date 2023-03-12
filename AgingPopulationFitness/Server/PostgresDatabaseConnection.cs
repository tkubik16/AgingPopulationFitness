using Npgsql;
using System;


namespace AgingPopulationFitness.Server
{
    
    public sealed class PostgresDatabaseConnection
    {
        private static readonly Lazy<PostgresDatabaseConnection> lazyInstance =
        new Lazy<PostgresDatabaseConnection>(() => new PostgresDatabaseConnection());

        private readonly string connectionString;

        private readonly DatabaseCredentials databaseCredentials = new DatabaseCredentials();

        private PostgresDatabaseConnection()
        {
            // Initialize the PostgreSQL database connection
            connectionString = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + ";" +
                "Application Name=" + "UserController" + ";" +
                "Pooling=" + DatabaseCredentials.Pooling + ";" +
                "Maximum Pool Size=" + DatabaseCredentials.MaxPoolSize + ";" +
                "Minimum Pool Size=" + DatabaseCredentials.MinPoolSize + "";


            
        }

        public static PostgresDatabaseConnection Instance
        {
            get { return lazyInstance.Value; }
        }

        public NpgsqlConnection GetConnection()
        {
            // Create a new connection object for each method call
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }

}


