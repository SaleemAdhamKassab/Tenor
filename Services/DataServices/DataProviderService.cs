using Azure;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using Tenor.Data;
using Tenor.Helper;

namespace Tenor.Services.DataServices
{
    public interface IDataProviderService
    {
        public List<string> fetchFilterOptionsByQuery(string query);
        IEnumerable<dynamic> fetchData(string query);
    }
    public class DataProviderService : IDataProviderService
    {
        private readonly DataContext _dDb;
        public DataProviderService(DataContext dDb)
        {
            _dDb = dDb;
        }

        public List<string> fetchFilterOptionsByQuery(string query)
        {
            var result = _dDb.Database.SqlQueryRaw<string>($"{query}").ToList();
            return result ?? new List<string>();
        }
        //public async List<dynamic> fetchData(string query)
        //{
        //    var conn = _dDb.Database.GetDbConnection();
        //    using(var command = conn.CreateCommand())
        //    {
        //        command.CommandText = query;
        //        await conn.OpenAsync();
        //        IDataReader reader = await command.ExecuteReaderAsync();
        //        while(reader.Read())
        //        {
        //            var row = new ExpandoObject() as IDictionary<string, object>;
        //            for (var fieldCount = 0; fieldCount < reader.FieldCount; fieldCount++)
        //            {
        //                row.Add(reader.GetName(fieldCount), reader[fieldCount]);
        //            }
        //            yield return row;
        //        }
        //    }
        //    //var result = _dDb.Database.SqlQueryRaw<Dictionary<string,string>>($"{query}").ToList<object>();
        //    //return result ?? new List<object>();
        //}
        public IEnumerable<dynamic> fetchData(string query)
        {
            using (var cmd = _dDb.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = query;
                if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }

                //foreach (KeyValuePair<string, object> p in Params)
                //{
                //    DbParameter dbParameter = cmd.CreateParameter();
                //    dbParameter.ParameterName = p.Key;
                //    dbParameter.Value = p.Value;
                //    cmd.Parameters.Add(dbParameter);
                //}

                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var row = new ExpandoObject() as IDictionary<string, object>;
                        for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                        {
                            row.Add(dataReader.GetName(fieldCount), dataReader[fieldCount]);
                        }
                        yield return row;
                    }
                }
            }
        }
    }
}
