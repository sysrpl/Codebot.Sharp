using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using Map = System.Collections.Generic.Dictionary<string, object>;

namespace Codebot.Data
{
    public class DataCommand
    {
        private string query;
        private int timeout;
        private DataParameters parameters;

        public static DataCommand Prepare(string query, bool resource = false)
        {
            DataCommand command = new DataCommand();
            command.query = resource ? DataConnect.LoadResourceText(Assembly.GetCallingAssembly(), query) : query;
            command.parameters = null;
            command.timeout = 30;
            return command;
        }

        public DataCommand Add(string name, object value)
        {
            if (parameters == null)
                parameters = new DataParameters();
            parameters.Add(name, value);
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

        public DbDataReader ExecuteReader()
        {
            return DataConnect.ExecuteReader(query, false, timeout, parameters);
        }

        public IEnumerable<Map> ExecuteMaps()
        {
            using (var reader = ExecuteReader())
            {
                var maps = new List<Map>();
                var columns = new List<string>();
                for (var i = 0; i < reader.FieldCount; i++)
                    columns.Add(reader.GetName(i));
                while (reader.Read())
                {
                    var map = new Map();
                    foreach (var column in columns)
                        map.Add(column, reader[column]);
                    maps.Add(map);
                }
                return maps;
            }
        }

        public IEnumerable<T> Compose<T>(Func<DbDataReader, T> selector)
        {
            using (var reader = DataConnect.ExecuteReader(query, false, timeout, parameters))
                return reader.Compose(selector).ToList();
        }

        public bool ExecuteSingleResult<T>(out T result)
        {
            result = default(T);
            using (var reader = DataConnect.ExecuteReader(query, false, timeout, parameters))
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
            return DataConnect.ExecuteNonQuery(query, false, timeout, parameters);
        }
    }
}

