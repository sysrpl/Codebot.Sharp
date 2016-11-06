using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Codebot.Data
{
    public class DataReplicator
    {
        private string source;
        private string dest;
        private Action<string> feedback;
        private Func<bool> terminated;

        public DataReplicator(string sourceConnectionString, string destConnectionString, 
            Action<string> writeFeedback, Func<bool> checkTerminated)
        {
            source = sourceConnectionString;
            dest = destConnectionString;
            feedback = writeFeedback;
            terminated = checkTerminated;
        }

        public void Replicate(string tableName, string keyName, ref long keyId, int breakOn = 1)
        {
            if (terminated())
                return;
            DataConnect.ConnectionString = source;
            var columns = new List<string>();
            using (var reader = DataCommand.Prepare("select top 1 * from " + tableName).ExecuteReader())
                for (int i = 0; i < reader.FieldCount; i++)
                    columns.Add(reader.GetName(i));
            string selectSql = String.Format("select {0} from {1} where {2} > {3} order by {2}", 
                String.Join(", ", columns), tableName, keyName, keyId);
            string insertSql = String.Format("insert into {0} ({1}) values (@{2})", 
                tableName, String.Join(", ", columns), String.Join(", @", columns));
            using (var reader = DataCommand.Prepare(selectSql).ExecuteReader())
            {
                long records = 0;
                long batch = 0;
                int count = 0;
                DateTime timeA = DateTime.Now;
                DataConnect.ConnectionString = dest;
                while (reader.Read())
                {
                    records++;
                    batch++;
                    keyId = reader.ReadLong(keyName);
                    var command = DataCommand.Prepare(insertSql);
                    for (int i = 0; i < reader.FieldCount; i++)
                        command.Add("@" + columns[i], reader.GetValue(i));
                    command.ExecuteNonQuery();
                    DateTime timeB = DateTime.Now;
                    if ((timeB - timeA).TotalSeconds > 5)
                    {
                        timeA = timeB;
                        feedback(String.Format("{0:n0} {1} replicated", records, tableName));
                        feedback(String.Format("{0} records per second", batch / 5));
                        batch = 0;
                    }
                    count++;
                    if (count == breakOn)
                    {
                        count = 0;
                        if (terminated())
                            break;
                    }
                }
                feedback(String.Format("Completed: {0:n0} {1} replicated", records, tableName));
            }
        }
    }
}

