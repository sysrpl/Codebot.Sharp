using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

namespace Codebot.Data
{
    public static class DataConnect
    {
        static DataConnect()
        {
            ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog={0};Integrated Security=SSPI;Pooling=True";
        }

        private static SqlConnection CreateConnection()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            return connection;
        }

        public static string ConnectionString { get; set; }

        public static string LoadResourceText(Assembly assembly, string name)
        {
            string resource = assembly.GetManifestResourceNames().First(item => item.EndsWith(name));
            using (Stream s = assembly.GetManifestResourceStream(resource))
            using (StreamReader reader = new StreamReader(s))
                return reader.ReadToEnd();
        }

        public static string LoadResourceText(string name)
        {
            return LoadResourceText(Assembly.GetCallingAssembly(), name);
        }

        public static SqlDataReader ExecuteReader(string query, bool resource = false, int timeout = 30)
        {
			if (resource)
				query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            return ExecuteReader(query, false, timeout, new SqlParameter[] { });
        }

        public static SqlDataReader ExecuteReader(string query, bool resource, params SqlParameter[] parameters)
        {
            if (resource)
                query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            return ExecuteReader(query, false, 30, parameters);
        }

        public static SqlDataReader ExecuteReader(string query, bool resource, int timeout, params SqlParameter[] parameters)
        {
            if (resource)
                query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            SqlConnection connection = CreateConnection();
            connection.Open();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddRange(parameters);
                SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return reader;
            }
        }

        public static int ExecuteNonQuery(string query, bool resource = false, int timeout = 30)
        {
			if (resource)
				query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            return ExecuteNonQuery(query, false, timeout, new SqlParameter[] { });
        }

        public static int ExecuteNonQuery(string query, bool resource, params SqlParameter[] parameters)
        {
            if (resource)
                query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            return ExecuteNonQuery(query, false, 30, parameters);
        }

        public static int ExecuteNonQuery(string query, bool resource, int timeout, params SqlParameter[] parameters)
        {
            if (resource)
                query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandTimeout = timeout;
                    command.Parameters.AddRange(parameters);
                    return command.ExecuteNonQuery();
                }
            }
        }
    }
}