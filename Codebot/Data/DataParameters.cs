using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Codebot
{
    public class DataParameters
    {
        private static Dictionary<Type, DbType> map;

        static DataParameters()
        {
            map = new Dictionary<Type, DbType>();
            map[typeof(byte)] = DbType.Byte;
            map[typeof(sbyte)] = DbType.SByte;
            map[typeof(short)] = DbType.Int16;
            map[typeof(ushort)] = DbType.UInt16;
            map[typeof(int)] = DbType.Int32;
            map[typeof(uint)] = DbType.UInt32;
            map[typeof(long)] = DbType.Int64;
            map[typeof(ulong)] = DbType.UInt64;
            map[typeof(float)] = DbType.Single;
            map[typeof(double)] = DbType.Double;
            map[typeof(decimal)] = DbType.Decimal;
            map[typeof(bool)] = DbType.Boolean;
            map[typeof(string)] = DbType.String;
            map[typeof(char)] = DbType.StringFixedLength;
            map[typeof(Guid)] = DbType.Guid;
            map[typeof(DateTime)] = DbType.DateTime;
            map[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            map[typeof(byte[])] = DbType.Binary;
            map[typeof(byte?)] = DbType.Byte;
            map[typeof(sbyte?)] = DbType.SByte;
            map[typeof(short?)] = DbType.Int16;
            map[typeof(ushort?)] = DbType.UInt16;
            map[typeof(int?)] = DbType.Int32;
            map[typeof(uint?)] = DbType.UInt32;
            map[typeof(long?)] = DbType.Int64;
            map[typeof(ulong?)] = DbType.UInt64;
            map[typeof(float?)] = DbType.Single;
            map[typeof(double?)] = DbType.Double;
            map[typeof(decimal?)] = DbType.Decimal;
            map[typeof(bool?)] = DbType.Boolean;
            map[typeof(char?)] = DbType.StringFixedLength;
            map[typeof(Guid?)] = DbType.Guid;
            map[typeof(DateTime?)] = DbType.DateTime;
            map[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
        }

        public static void Build(DbCommand command, DataParameters parameters)
        {
            command.Parameters.Clear();
            if (parameters == null)
                return;
            foreach (var pair in parameters.values)
            {
                var p = command.CreateParameter();
                p.DbType = map[pair.Key.GetType()];
                p.ParameterName = pair.Key;
                p.Value = pair.Value;
                command.Parameters.Add(p);
            }
        }

        private Dictionary<string, object> values;

        public DataParameters()
        {
          values = new Dictionary<string, object>();
        }

        public object this[string key]
        {
            get
            {
                return values[key];
            }
            set
            {
                Add(key, value);
            }
        }

        public DataParameters Add(string key, object value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);
            return this;
        }
    }
}
