using System;
using System.Data.SqlClient;

namespace WF_CW2_TEST.Infrastructure
{
    internal static class Database
    {
        private const string DefaultConnectionString =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ZMC_Academy;Integrated Security=True";

        public static string ConnectionString
        {
            get
            {
                string configured = Environment.GetEnvironmentVariable("ZMC_DB_CONNECTION");
                return string.IsNullOrWhiteSpace(configured)
                    ? DefaultConnectionString
                    : configured;
            }
        }

        public static SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }
    }
}
