using Npgsql;
using System;


namespace AgingPopulationFitness.Server
{
    
    public sealed class PostgresDatabaseDataSource
    {
        private readonly DatabaseCredentials databaseCredentials = new DatabaseCredentials();
        private static readonly Lazy<PostgresDatabaseDataSource> lazyInstance = new Lazy<PostgresDatabaseDataSource>(() => new PostgresDatabaseDataSource());
        private readonly string connectionString = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + ";" +
                "Application Name=" + "UserController" + ";" +
                "Pooling=" + DatabaseCredentials.Pooling + ";" +
                "Maximum Pool Size=" + DatabaseCredentials.MaxPoolSize + ";" +
                "Minimum Pool Size=" + DatabaseCredentials.MinPoolSize + "";
        private readonly NpgsqlDataSource dataSource;

        private PostgresDatabaseDataSource()
        {
            dataSource = NpgsqlDataSource.Create(connectionString);
        }

        public static PostgresDatabaseDataSource Instance
        {
            get
            {
                return lazyInstance.Value;
            }
        }

        public async Task<NpgsqlConnection> GetConnection()
        {
            return await dataSource.OpenConnectionAsync();
        }
    }

}


