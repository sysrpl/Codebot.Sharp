using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;

namespace Codebot.Data
{
    public class DataCommand
    {
        private string query;
        private int timeout;
        private List<SqlParameter> parameters;

        public static DataCommand Prepare(string query, bool resource = false)
        {
            DataCommand command = new DataCommand();
			command.query = resource ? DataConnect.LoadResourceText(Assembly.GetCallingAssembly(), query) : query;
            command.parameters = new List<SqlParameter>();
            command.timeout = 30;
            return command;
        }

        public DataCommand Add(string name, object value)
        {
            parameters.Add(new SqlParameter(name, value));
            return this;
        }

        public DataCommand Timeout(int value)
        {
            timeout = value;
            return this;
        }

        public bool ExecuteHasRows()
        {
            using (var reader = ExecuteReader())
                return reader.HasRows;
        }

        public SqlDataReader ExecuteReader()
        {
            return DataConnect.ExecuteReader(query, false, timeout, parameters.ToArray());
        }


        public IEnumerable<T> Compose<T>(Func<DbDataReader, T> selector)
        {
            using (var reader = DataConnect.ExecuteReader(query, false, timeout, parameters.ToArray()))
                return reader.Compose(selector).ToList();
        }

        public bool ExecuteSingleResult<T>(out T result)
        {
            result = default(T);
            using (var reader = DataConnect.ExecuteReader(query, false, timeout, parameters.ToArray()))
                if (reader.Read())
                {
                    result = reader.Read<T>(0);
                    return true;
                }
                else
                    return false;
        }

        public int ExecuteNonQuery()
        {
            return DataConnect.ExecuteNonQuery(query, false, timeout, parameters.ToArray());
        }
    }
}

