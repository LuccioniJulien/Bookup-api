using System;

namespace BaseApi.Helper
{
    public class DbConfig
    {
        private string _dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        private string _dbName = Environment.GetEnvironmentVariable("DB_NAME");
        private string _dbUser = Environment.GetEnvironmentVariable("DB_USER");
        private string _dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
        public void Deconstruct(out string dbHost, out string dbName, out string dbUser, out string dbPassword)
        {
            dbHost = _dbHost;
            dbName = _dbName;
            dbUser = _dbUser;
            dbPassword = _dbPassword;
        }
    }
}