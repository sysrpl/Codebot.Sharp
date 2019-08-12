using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Data.Common;
using System.Collections.Generic;

namespace Codebot.Data
{
    public delegate void DataRead(DbDataReader reader);

    public static class DataConnect
    {
        public static Func<DbConnection> CreateConnection { get; set; }

        public static string ConnectionString { get; set; }

        public static string LoadResourceText(Assembly assembly, string name)
        {
            string resource = assembly.GetManifestResourceNames().First(item => 
                item.EndsWith(name, StringComparison.Ordinal));
            using (Stream s = assembly.GetManifestResourceStream(resource))
            using (StreamReader reader = new StreamReader(s))
                return reader.ReadToEnd();
        }

        public static string LoadResourceText(string name)
        {
            return LoadResourceText(Assembly.GetCallingAssembly(), name);
        }

        public static void ExecuteReader(DataRead read, string query, bool resource = false, int timeout = 30, DataParameters parameters = null)
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
                    using (var reader = command.ExecuteReader())
                        read(reader);
                }
            }
        }

        public static void ExecuteReader(DataRead read, string query, bool resource, DataParameters parameters)
        {
            if (resource)
                query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            ExecuteReader(read, query, false, 30, parameters);
        }

        public static int ExecuteNonQuery(string query, bool resource = false, int timeout = 30, DataParameters parameters = null)
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

        public static int ExecuteNonQuery(string query, bool resource, DataParameters parameters = null)
        {
            if (resource)
                query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            return ExecuteNonQuery(query, false, 30, parameters);
        }

        public static IEnumerable<T> ExecuteComposer<T>(Func<DbDataReader, T> composer, 
            string query, bool resource = false, DataParameters parameters = null)
        {
            IEnumerable<T> list = null;
            if (resource)
                query = LoadResourceText(Assembly.GetCallingAssembly(), query);
            ExecuteReader(r => list = r.Compose(composer), query, false, parameters);
            return list;
        }
    }
}