using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Data;
using System.Data.Common;

namespace Codebot.Data
{
    public static class DataConnect
    {
        public static Func<DbConnection> CreateConnection { get; set; }

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

        public static DbDataReader ExecuteReader(string query, bool resource = false, int timeout = 30)
        {
            if (resource)
                query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            return ExecuteReader(query, false, timeout, null);
        }

        public static DbDataReader ExecuteReader(string query, bool resource, DataParameters parameters)
        {
            if (resource)
                query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            return ExecuteReader(query, false, 30, parameters);
        }

        public static DbDataReader ExecuteReader(string query, bool resource, int timeout, DataParameters parameters = null)
        {
            if (resource)
                query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            var connection = CreateConnection();
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandTimeout = timeout;
                DataParameters.Build(command, parameters);
                return command.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }

        public static int ExecuteNonQuery(string query, bool resource = false, int timeout = 30)
        {
            if (resource)
                query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            return ExecuteNonQuery(query, false, timeout, null);
        }

        public static int ExecuteNonQuery(string query, bool resource, DataParameters parameters = null)
        {
            if (resource)
                query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            return ExecuteNonQuery(query, false, 30, parameters);
        }

        public static int ExecuteNonQuery(string query, bool resource, int timeout, DataParameters parameters = null)
        {
            if (resource)
                query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandTimeout = timeout;
                    DataParameters.Build(command, parameters);
                    return command.ExecuteNonQuery();
                }
            }
        }
    }
}