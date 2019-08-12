using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;

namespace Codebot.Data
{
	public static class DataReaderExtensions
	{
        public static bool IsNumber(this object value)
        {
            return value is sbyte
                || value is byte
                || value is short
                || value is ushort
                || value is int
                || value is uint
                || value is long
                || value is ulong
                || value is float
                || value is double
                || value is decimal;
        }

        private static bool ReadBool(object value)
        {
            if (value.IsNumber())
            {
                int i = (int)value;
                return i != 0;
            }
            var s = value.ToString().ToUpper(); 
            switch (s)
            {
                case "Y":
                case "YES":
                case "T":
                case "TRUE":
                    return true;
                default:
                    return false;
            }
        }

        public static bool ReadBool(this DbDataReader dataReader, string column)
        {
            int i = dataReader.GetOrdinal(column);
            if (dataReader.IsDBNull(i))
                return false;
            else
                return ReadBool(dataReader.GetValue(i));
        }

        public static bool ReadBool(this DbDataReader dataReader, int column)
        {
            if (dataReader.IsDBNull(column))
                return false;
            else
                return ReadBool(dataReader.GetValue(column));
        }

        public static string ReadString(this DbDataReader dataReader, string column)
		{
			int i = dataReader.GetOrdinal(column);
			if (dataReader.IsDBNull(i))
				return string.Empty;
			else
				return dataReader.GetValue(i).ToString();
		}

		public static string ReadString(this DbDataReader dataReader, int column)
		{
			if (dataReader.IsDBNull(column))
				return string.Empty;
			else
				return dataReader.GetValue(column).ToString();
		}

		public static int ReadInt(this DbDataReader dataReader, string column)
		{
			int i = dataReader.GetOrdinal(column);
			if (dataReader.IsDBNull(i))
				return 0;
			else
				return dataReader.GetInt32(i);
		}

		public static int ReadInt(this DbDataReader dataReader, int column)
		{
			if (dataReader.IsDBNull(column))
				return 0;
			else
				return dataReader.GetInt32(column);
		}

        public static Int64 ReadLong(this DbDataReader dataReader, string column)
        {
            int i = dataReader.GetOrdinal(column);
            if (dataReader.IsDBNull(i))
                return 0;
            else
                return dataReader.GetInt64(i);
        }

        public static Int64 ReadLong(this DbDataReader dataReader, int column)
        {
            if (dataReader.IsDBNull(column))
                return 0;
            else
                return dataReader.GetInt64(column);
        }

        public static double ReadFloat(this DbDataReader dataReader, string column)
        {
            int i = dataReader.GetOrdinal(column);
            if (dataReader.IsDBNull(i))
                return 0;
            else
                return dataReader.GetDouble(i);
        }

        public static double ReadFloat(this DbDataReader dataReader, int column)
        {
            if (dataReader.IsDBNull(column))
                return 0;
            else
                return dataReader.GetDouble(column);
        }

        public static DateTime ReadDate(this DbDataReader dataReader, string column)
        {
            return dataReader.Read<DateTime>(column);
        }

        public static DateTime ReadDate(this DbDataReader dataReader, int column)
        {
            return dataReader.Read<DateTime>(column);
        }

        public static T Read<T>(this DbDataReader dataReader, string column)
        {
            int i = dataReader.GetOrdinal(column);
            if (dataReader.IsDBNull(i))
                return default(T);
            else
                return (T)dataReader[i];
        }

        public static T Read<T>(this DbDataReader dataReader, int column)
        {
            if (dataReader.IsDBNull(column))
                return default(T);
            else
                return (T)dataReader[column];
        }

        static IEnumerable<T> ComposeDirect<T>(this DbDataReader reader, Func<DbDataReader, T> composer)
        {
            while (reader.Read())
                yield return composer(reader);
        }

        public static IEnumerable<T> Compose<T>(this DbDataReader reader, Func<DbDataReader, T> composer)
        {
            return reader.ComposeDirect(composer).ToList();
        }
    }
}
